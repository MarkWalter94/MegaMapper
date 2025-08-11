using System.Collections;
using System.Collections.Concurrent;
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
    private readonly List<IMegaMapperProfile> _profiles = new();

    public MegaMapper(IEnumerable<IMegaMapperProfile> profiles, IEnumerable<IMegaMapperMapBuilder> builders)
    {
        _profiles.AddRange(profiles);
        _profiles.AddRange(builders.Select(x => x.BuildProfile()));
    }

    public async Task<TOut> Map<TOut>(object input)
    {
        return (TOut)(await Map(typeof(TOut), input))!;
    }

    public async Task<TOut> Map<TIn, TOut>(TIn input)
    {
        return await Map<TOut>(input!);
    }

    private async Task<object?> Map(Type outputType, object? input, Dictionary<object, object?>? mappedObjects = null)
    {
        if (input == null)
            return null;

        //For recurring objects and cycles.
        mappedObjects ??= new Dictionary<object, object?>(new ReferenceEqualityComparer());

        if (mappedObjects.TryGetValue(input, out var existingMapped))
            return existingMapped;

        var matchedProfiles = _profiles.Where(mapperProfile =>
            mapperProfile.GetTIn() == input.GetType() && mapperProfile.GetTOut() == outputType).ToList();

        //If there is a profile use it.
        if (matchedProfiles.Count != 0)
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

        var inverseProfiles = _profiles.Where(mapperProfile =>
            mapperProfile.GetTIn() == outputType && mapperProfile.GetTOut() == input.GetType()).ToList();

        // If there is a inverse profile use it.
        if (inverseProfiles.Count > 0)
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

    private async Task<object?> AutoMap(Type outputType, object? input, Dictionary<object, object?>? mappedObjects = null)
    {
        if (input == null)
            return null;
        // Mapping for enumerables.
        if (typeof(IEnumerable).IsAssignableFrom(outputType) && input is IEnumerable inputEnumerable)
        {
            var outElementType = outputType.IsGenericType
                ? outputType.GetGenericArguments()[0]
                : typeof(object); // fallback if non generic

            var resultList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(outElementType))!;

            foreach (var item in inputEnumerable)
            {
                var mappedItem = await Map(outElementType, item, mappedObjects);
                resultList.Add(mappedItem);
            }

            return resultList;
        }


        // Base map for non mapped types.
        var output = Activator.CreateInstance(outputType);
        mappedObjects![input] = output;

        var inputProperties = GetOrAddProperties(input.GetType());
        var outputProperties = GetOrAddProperties(outputType);

        foreach (var inProperty in inputProperties.Values)
        {
            if (!inProperty.CanRead) continue;
            if (!outputProperties.TryGetValue(inProperty.Name.ToLower(), out var outProperty)) continue;
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
            if (targetType.IsAssignableFrom(sourceType) || typeof(IConvertible).IsAssignableFrom(targetType))
            {
                outProperty.SetValue(output, Convert.ChangeType(value, targetType));
                continue;
            }

            // Recurring mapping if none of these
            outProperty.SetValue(output, await Map(outProperty.PropertyType, value, mappedObjects));
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
            t.GetProperties().ToDictionary(prop => prop.Name.ToLower()));
    }
}