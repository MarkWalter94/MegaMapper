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
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertiesCache = new();
    private readonly Dictionary<Tuple<Type, Type>, List<IMegaMapperProfile>> _profiles = new();
    private static readonly ConcurrentDictionary<Type, Func<object>> _ctorCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, Dictionary<object, object?>, object>> _compiledMaps = new();

    public MegaMapper(IEnumerable<IMegaMapperProfile> inputProfiles, IEnumerable<IMegaMapperMapBuilder> builders)
    {
        var profiles = builders.Select(x => x.BuildProfile()).ToList();
        profiles.AddRange(inputProfiles);

        foreach (var profile in profiles)
        {
            var key = Tuple.Create(profile.GetTIn(), profile.GetTOut());
            if (_profiles.TryGetValue(key, out var list))
                list.Add(profile);
            else
                _profiles[key] = new List<IMegaMapperProfile> { profile };
        }
    }

    public async Task<TOut> Map<TOut>(object input) => (TOut)(await Map(typeof(TOut), input))!;
    public async Task<TOut> Map<TIn, TOut>(TIn input) => await Map<TOut>(input!);

    private async Task<object?> Map(Type outputType, object? input)
    {
        return await Map(outputType, input, new Dictionary<object, object?>(new ReferenceEqualityComparer()));
    }

    private async Task<object?> Map(Type outputType, object? input, Dictionary<object, object?> mappedObjects)
    {
        if (input == null) return null;
        if (mappedObjects.TryGetValue(input, out var existing)) return existing;

        var inputType = input.GetType();

        // Forward profiles
        if (_profiles.TryGetValue(Tuple.Create(inputType, outputType), out var matchedProfiles) && matchedProfiles.Count > 0)
        {
            var pass = matchedProfiles.Any(x => x.UseBaseMap)
                ? DeepMap(input, outputType, mappedObjects)
                : CreateInstance(outputType);

            foreach (var profile in matchedProfiles)
                pass = await profile.MapInternal(input, pass);

            return pass;
        }

        // Reverse profiles
        if (_profiles.TryGetValue(Tuple.Create(outputType, inputType), out var inverseProfiles) && inverseProfiles.Count > 0)
        {
            var pass = inverseProfiles.Any(x => x.UseBaseMap)
                ? DeepMap(input, outputType, mappedObjects)
                : CreateInstance(outputType);

            foreach (var profile in inverseProfiles)
                pass = await profile.MapBackInternal(input, pass);

            return pass;
        }

        // No profiles → generic deep map 
        return DeepMap(input, outputType, mappedObjects);
    }

    /// <summary>
    /// Maps “in depth” by compiling a lambda for (sourceType, targetType).
    /// Supports: simple types via assign/convert, complex objects via property copy,
    /// 1D arrays, jagged arrays ([][]…), and rectangular matrices (T[,], T[,,], …).
    /// </summary>
    private object DeepMap(object input, Type outputType, Dictionary<object, object?> mappedObjects)
    {
        if (mappedObjects.TryGetValue(input, out var existing))
            return existing!;

        var inputType = input.GetType();

        // Create instance to avoid loops..
        var instance = CreateInstance(outputType);
        mappedObjects[input] = instance;

        var func = _compiledMaps.GetOrAdd((inputType, outputType), key =>
        {
            var (sourceType, targetType) = key;

            var srcParam = Expression.Parameter(typeof(object), "src");
            var ctxParam = Expression.Parameter(typeof(Dictionary<object, object?>), "ctx");

            var srcVar = Expression.Variable(sourceType, "typedSrc");
            var tgtVar = Expression.Variable(targetType, "tgt");

            var expressions = new List<Expression>();

            // typedSrc = (TSource)src;
            expressions.Add(Expression.Assign(srcVar, Expression.Convert(srcParam, sourceType)));

            // tgt = (TTarget)CreateInstance(targetType);
            expressions.Add(Expression.Assign(
                tgtVar,
                Expression.Convert(
                    Expression.Call(typeof(MegaMapper)
                            .GetMethod(nameof(CreateInstance), BindingFlags.Static | BindingFlags.NonPublic)!,
                        Expression.Constant(targetType)
                    ),
                    targetType
                )
            ));

            // ctx[src] = tgt;
            var dictIndexer = typeof(Dictionary<object, object?>).GetProperty("Item");
            expressions.Add(Expression.Assign(
                Expression.Property(ctxParam, dictIndexer!, Expression.Convert(srcVar, typeof(object))),
                Expression.Convert(tgtVar, typeof(object))
            ));

            var srcProps = GetOrAddProperties(sourceType);
            var tgtProps = GetOrAddProperties(targetType);

            foreach (var sp in srcProps.Values)
            {
                if (!sp.CanRead || sp.GetIndexParameters().Length > 0) continue;
                if (!tgtProps.TryGetValue(sp.Name, out var tp) || !tp.CanWrite || tp.GetIndexParameters().Length > 0) continue;

                var srcPropExpr = Expression.Property(srcVar, sp);
                var tgtPropExpr = Expression.Property(tgtVar, tp);

                var sType = sp.PropertyType;
                var tType = tp.PropertyType;

                Expression assignExpression;

                if (sType.IsArray && tType.IsArray)
                {
                    assignExpression = Expression.Assign(
                        tgtPropExpr,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                typeof(MegaMapper).GetMethod(nameof(MapArray), BindingFlags.NonPublic | BindingFlags.Instance)!,
                                Expression.Convert(srcPropExpr, typeof(Array)),
                                Expression.Constant(tType),
                                ctxParam
                            ),
                            tType
                        )
                    );
                }
                else if (typeof(IEnumerable).IsAssignableFrom(sType) && typeof(IEnumerable).IsAssignableFrom(tType)
                                                                     && sType != typeof(string) && tType != typeof(string))
                {
                    assignExpression = Expression.Assign(
                        tgtPropExpr,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                typeof(MegaMapper).GetMethod(nameof(MapEnumerable), BindingFlags.NonPublic | BindingFlags.Instance)!,
                                Expression.Convert(srcPropExpr, typeof(object)),
                                Expression.Constant(tType),
                                ctxParam
                            ),
                            tType
                        )
                    );
                }
                else if (sType != tType && !sType.IsValueType && !tType.IsValueType &&
                         sType != typeof(string) && tType != typeof(string))
                {
                    assignExpression = Expression.Assign(
                        tgtPropExpr,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                typeof(MegaMapper).GetMethod(nameof(DeepMap), BindingFlags.NonPublic | BindingFlags.Instance)!,
                                Expression.Convert(srcPropExpr, typeof(object)),
                                Expression.Constant(tType),
                                ctxParam
                            ),
                            tType
                        )
                    );
                }
                else
                {
                    assignExpression = Expression.Assign(tgtPropExpr, Expression.Convert(srcPropExpr, tType));
                }

                expressions.Add(
                    sType.IsValueType
                        ? assignExpression
                        : Expression.IfThen(
                            Expression.NotEqual(srcPropExpr, Expression.Constant(null, sType)),
                            assignExpression
                        )
                );
            }

            expressions.Add(tgtVar);

            var body = Expression.Block(new[] { srcVar, tgtVar }, expressions);
            return Expression.Lambda<Func<object, Dictionary<object, object?>, object>>(body, srcParam, ctxParam).Compile();
        });

        return func(input, mappedObjects);
    }

    private object? MapEnumerable(object? sourceEnumerable, Type targetEnumerableType, Dictionary<object, object?> ctx)
    {
        if (sourceEnumerable == null) return null;

        var srcEnum = ((IEnumerable)sourceEnumerable).Cast<object>();
        var dest = (IList)Activator.CreateInstance(targetEnumerableType)!;

        var targetElementType = targetEnumerableType.IsGenericType
            ? targetEnumerableType.GetGenericArguments()[0]
            : typeof(object);

        foreach (var item in srcEnum)
        {
            object? mappedElem;
            if (item == null)
            {
                mappedElem = null;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(targetElementType) && targetElementType != typeof(string))
            {
                // sublist → recursion
                mappedElem = MapEnumerable(item, targetElementType, ctx);
            }
            else
            {
                // simple type
                mappedElem = DeepMap(item, targetElementType, ctx);
            }

            dest.Add(mappedElem);
        }

        return dest;
    }


    /// <summary>
    /// Maps a source array (including multi-dimensional arrays) to an array of the destination type.
    /// Supports: 1D, jagged ([][]... recursive), and rectangular (Rank > 1).
    /// </summary>
    private object? MapArray(Array? sourceArray, Type targetArrayType, Dictionary<object, object?> ctx)
    {
        if (sourceArray is null) return null;

        var targetElementType = targetArrayType.GetElementType()!;
        var rank = sourceArray.Rank;

        if (rank == 1)
        {
            var len = sourceArray.Length;
            var dest = Array.CreateInstance(targetElementType, len);

            for (int i = 0; i < len; i++)
            {
                var srcElem = sourceArray.GetValue(i);
                object? mappedElem;

                if (srcElem is null)
                {
                    mappedElem = null;
                }
                else if (srcElem is Array innerSrc && targetElementType.IsArray)
                {
                    // Jagged: element is itself an array → recursive
                    mappedElem = MapArray(innerSrc, targetElementType, ctx);
                }
                else
                {
                    // “Normal” element: deep map to the destination element type
                    mappedElem = DeepMap(srcElem, targetElementType, ctx);
                }

                dest.SetValue(mappedElem, i);
            }

            return dest;
        }
        else
        {
            // Rectangular: T[,], T[,,], ...
            var lengths = Enumerable.Range(0, rank).Select(d => sourceArray.GetLength(d)).ToArray();
            var dest = Array.CreateInstance(targetElementType, lengths);

            // Iterate through all indexes with a vector counter
            var indices = new int[rank];
            var done = false;

            while (!done)
            {
                var srcElem = sourceArray.GetValue(indices);
                object? mappedElem;

                if (srcElem is null)
                {
                    mappedElem = null;
                }
                else if (srcElem is Array inner && targetElementType.IsArray)
                {
                    mappedElem = MapArray(inner, targetElementType, ctx);
                }
                else
                {
                    mappedElem = DeepMap(srcElem, targetElementType, ctx);
                }

                dest.SetValue(mappedElem, indices);

// Increases the index vector
                for (int dim = rank - 1; dim >= 0; dim--)
                {
                    indices[dim]++;
                    if (indices[dim] < lengths[dim])
                    {
                        break;
                    }
                    else
                    {
                        indices[dim] = 0;
                        if (dim == 0) done = true;
                    }
                }
            }

            return dest;
        }
    }

    private static object CreateInstance(Type type)
    {
        return _ctorCache.GetOrAdd(type, t =>
            Expression.Lambda<Func<object>>(Expression.New(t)).Compile()
        )();
    }

    private static Dictionary<string, PropertyInfo> GetOrAddProperties(Type type)
    {
        // NB: we do not filter the indexers here in order to have a “complete” cache;
        // we skip them in the expression build cycle.
        return _propertiesCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase));
    }

    private class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}