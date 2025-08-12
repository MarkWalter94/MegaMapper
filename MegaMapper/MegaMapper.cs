using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MegaMapper;

public interface IMegaMapper
{
    /// <summary>
    /// Does the map things.
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public Task<TOut> Map<TIn, TOut>(TIn input);


    /// <summary>
    /// Does the map things.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Task<TOut> Map<TOut>(object input);
}

public class MegaMapper : IMegaMapper
{
    // Cache for the properties of each type
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertiesCache = new();
    private readonly Dictionary<Tuple<Type, Type>, List<IMegaMapperProfile>> _profiles = new();
    private static readonly ConcurrentDictionary<Type, Func<object>> _ctorCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), bool> _assignableCache = new();
    private static readonly ConcurrentDictionary<Type, bool> _convertibleCache = new();
    private static readonly ConcurrentDictionary<Type, Func<IList>> _listFactoryCache = new();

    private static object CreateInstance(Type type)
    {
        return _ctorCache.GetOrAdd(type, t => Expression.Lambda<Func<object>>(Expression.New(t)).Compile())();
    }

    public MegaMapper(IEnumerable<IMegaMapperProfile> inputProfiles, IEnumerable<IMegaMapperMapBuilder> builders)
    {
        var profiles = builders.Select(x => x.BuildProfile()).ToList();
        profiles.AddRange(inputProfiles);

        foreach (var profile in profiles)
        {
            if (_profiles.TryGetValue(new Tuple<Type, Type>(profile.GetTIn(), profile.GetTOut()), out var properties))
                properties.Add(profile);
            else
                _profiles[new Tuple<Type, Type>(profile.GetTIn(), profile.GetTOut())] = [profile];
        }
    }

    public async Task<TOut> Map<TOut>(object input)
    {
        return (TOut)(await Map(typeof(TOut), input))!;
    }

    public async Task<TOut> Map<TIn, TOut>(TIn input)
    {
        return await Map<TOut>(input!);
    }

    private async Task<object?> Map(Type outputType, object? input)
    {
        return await Map(outputType, input, new Dictionary<object, object?>(new ReferenceEqualityComparer()));
    }


    private async Task<object?> Map(Type outputType, object? input, Dictionary<object, object?> mappedObjects)
    {
        if (input == null)
            return null;

        if (mappedObjects.TryGetValue(input, out var existingMapped))
            return existingMapped;

        if (_profiles.TryGetValue(new Tuple<Type, Type>(input.GetType(), outputType), out var matchedProfiles) && matchedProfiles.Count != 0)
        {
            var useBaseMap = matchedProfiles.Any(x => x.UseBaseMap);
            object? pass = null;
            if (useBaseMap)
            {
                pass = (await AutoMap(outputType, input, mappedObjects))!;
            }

            foreach (var matchedProfile in matchedProfiles)
            {
                if (pass != null)
                    pass = await matchedProfile.MapInternal(input, pass);
                else
                    pass = await matchedProfile.MapInternal(input);
            }

            return pass;
        }

        if (_profiles.TryGetValue(new Tuple<Type, Type>(outputType, input.GetType()), out var inverseProfiles) && inverseProfiles.Count > 0)
        {
            var useBaseMap = inverseProfiles.Any(x => x.UseBaseMap);

            object? pass = null;
            if (useBaseMap)
            {
                pass = (await AutoMap(outputType, input, mappedObjects))!;
            }

            foreach (var inverseProfile in inverseProfiles)
            {
                if (pass != null)
                    pass = await inverseProfile.MapBackInternal(input, pass);
                else
                    pass = await inverseProfile.MapBackInternal(input);
            }

            return pass;
        }

        return await AutoMap(outputType, input, mappedObjects);
    }

    private async Task<object?> AutoMap(Type outputType, object? input, Dictionary<object, object?> mappedObjects)
    {
        if (input == null)
            return null;

        if (typeof(IEnumerable).IsAssignableFrom(outputType) && input is IEnumerable inputEnumerable)
        {
            var outElementType = outputType.IsGenericType
                ? outputType.GetGenericArguments()[0]
                : typeof(object); // fallback if non generic
            
            var resultList = CreateListOfType(outElementType);

            foreach (var item in inputEnumerable)
            {
                var mappedItem = await Map(outElementType, item, mappedObjects);
                resultList.Add(mappedItem);
            }

            return resultList;
        }

        var output = CreateInstance(outputType);
        mappedObjects[input] = output;

        var inputProperties = GetOrAddProperties(input.GetType());
        var outputProperties = GetOrAddProperties(outputType);

        foreach (var inProperty in inputProperties.Values)
        {
            if (!inProperty.CanRead) continue;
            if (!outputProperties.TryGetValue(inProperty.Name, out var outProperty)) continue;
            if (!outProperty.CanWrite) continue;

            var value = inProperty.GetValue(input);

            if (value == null)
            {
                outProperty.SetValue(output, null);
                continue;
            }

            // If same type
            if (outProperty.PropertyType == inProperty.PropertyType)
            {
                outProperty.SetValue(output, value);
                continue;
            }
            // Nullable ↔ non nullable
            var targetType = Nullable.GetUnderlyingType(outProperty.PropertyType) ?? outProperty.PropertyType;
            var sourceType = Nullable.GetUnderlyingType(inProperty.PropertyType) ?? inProperty.PropertyType;

            if (targetType == sourceType)
            {
                // Same underlying type
                outProperty.SetValue(output, Convert.ChangeType(value, targetType));
                continue;
            }

            // Convertible types
            if (IsAssignableOrConvertible(targetType, sourceType))
            {
                outProperty.SetValue(output, Convert.ChangeType(value, targetType));
                continue;
            }

            // Recurring mapping if none of these
            var mapped = await Map(outProperty.PropertyType, value, mappedObjects);
            outProperty.SetValue(output, mapped);
        }

        return output;
    }

    private class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    /// <summary>
    /// Get or add cached properties references.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Dictionary<string, PropertyInfo> GetOrAddProperties(Type type)
    {
        return _propertiesCache.GetOrAdd(type, t =>
            t.GetProperties().ToDictionary(prop => prop.Name, StringComparer.OrdinalIgnoreCase));
    }

    private static readonly Type ConvertibleType = typeof(IConvertible);

    private static bool IsAssignableOrConvertible(Type targetType, Type sourceType)
    {
        var key = (targetType, sourceType);
        if (_assignableCache.TryGetValue(key, out bool result))
            return result;

        if (targetType.IsAssignableFrom(sourceType))
        {
            _assignableCache[key] = true;
            return true;
        }

        if (!_convertibleCache.TryGetValue(targetType, out bool isConv))
        {
            isConv = ConvertibleType.IsAssignableFrom(targetType);
            _convertibleCache[targetType] = isConv;
        }

        _assignableCache[key] = isConv;
        return isConv;
    }


    private static IList CreateListOfType(Type elementType)
    {
        return _listFactoryCache.GetOrAdd(elementType, t =>
        {
            var listType = typeof(List<>).MakeGenericType(t);
            var ctor = listType.GetConstructor(Type.EmptyTypes)!;

            var newExpr = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<IList>>(newExpr);
            return lambda.Compile();
        })();
    }
}