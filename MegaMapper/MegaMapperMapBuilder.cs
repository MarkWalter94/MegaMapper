using System.Linq.Expressions;

namespace MegaMapper;

/// <summary>
/// Interface for building simple maps.
/// </summary>
public interface IMegaMapperMapBuilder
{
    protected internal IMegaMapperProfile BuildProfile();
}

/// <summary>
/// Class for building simple maps.
/// </summary>
/// <typeparam name="TSrc"></typeparam>
/// <typeparam name="TDest"></typeparam>
public abstract class MegaMapperMapBuilder<TSrc, TDest> : IMegaMapperMapBuilder
{
    internal List<IPropertyMap<TSrc, TDest>> Maps { get; } = new();
    internal List<IPropertyMap<TDest, TSrc>> ReverseMaps { get; } = new();

    /// <summary>
    /// Identity mapping: Adds an automatic identity map from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// Also adds the return map.
    /// </summary>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> AutoMapField<TField>(Expression<Func<TSrc, TField>> inputFieldSelector, Expression<Func<TDest, TField>> outputFieldSelector)
    {
        var inputFieldMember = ReflectionHelper.FindProperty(inputFieldSelector);
        var outputFieldMember = ReflectionHelper.FindProperty(outputFieldSelector);
        Maps.Add(new PropertyMap<TSrc, TDest, TField, TField>(inputFieldMember, outputFieldMember, (_, _, input) => input));
        ReverseMaps.Add(new PropertyMap<TDest, TSrc, TField, TField>(outputFieldMember, inputFieldMember, (_, _, input) => input));
        return this;
    }

    /// <summary>
    /// Adds a map defined by the <paramref name="customMap"/> function from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapField<TField>(Expression<Func<TSrc, TField>> inputFieldSelector, Expression<Func<TDest, TField>> outputFieldSelector, Func<TSrc, TDest, TField, TField> customMap)
    {
        Maps.Add(new PropertyMap<TSrc, TDest, TField, TField>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }

    /// <summary>
    /// Adds a reverse map defined by the asynchronous function<paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    ///  to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapFieldBack<TField>(Expression<Func<TDest, TField>> inputFieldSelector, Expression<Func<TSrc, TField>> outputFieldSelector, Func<TDest, TSrc, TField, TField> customMap)
    {
        ReverseMaps.Add(new PropertyMap<TDest, TSrc, TField, TField>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }

    /// <summary>
    /// Adds a map defined by the asynchronous function<paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <remarks>
    /// Use this method when the mapping function is asynchronous and the input and output types match.
    /// </remarks>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapField<TField>(Expression<Func<TSrc, TField>> inputFieldSelector, Expression<Func<TDest, TField>> outputFieldSelector, Func<TSrc, TDest, TField, Task<TField>> customMap)
    {
        Maps.Add(new PropertyMap<TSrc, TDest, TField, TField>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }

    /// <summary>
    /// Adds a reverse map defined by the asynchronous function<paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <remarks>
    /// Use this method when the mapping function is asynchronous and the input and output types match.
    /// </remarks>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapFieldBack<TField>(Expression<Func<TDest, TField>> inputFieldSelector, Expression<Func<TSrc, TField>> outputFieldSelector, Func<TDest, TSrc, TField, Task<TField>> customMap)
    {
        ReverseMaps.Add(new PropertyMap<TDest, TSrc, TField, TField>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }

    /// <summary>
    /// Adds a map defined by the function <paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source and destination property types are different.
    /// </remarks>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TFieldSrc"></typeparam>
    /// <typeparam name="TFieldDst"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapField<TFieldSrc, TFieldDst>(Expression<Func<TSrc, TFieldSrc>> inputFieldSelector, Expression<Func<TDest, TFieldDst>> outputFieldSelector, Func<TSrc, TDest, TFieldSrc, TFieldDst> customMap)
    {
        Maps.Add(new PropertyMap<TSrc, TDest, TFieldSrc, TFieldDst>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }
    
    /// <summary>
    /// Adds a reverse map defined by the function <paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source and destination property types are different.
    /// </remarks>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TFieldSrc"></typeparam>
    /// <typeparam name="TFieldDst"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapFieldBack<TFieldSrc, TFieldDst>(Expression<Func<TDest, TFieldDst>> inputFieldSelector, Expression<Func<TSrc, TFieldSrc>> outputFieldSelector, Func<TDest, TSrc, TFieldDst, TFieldSrc> customMap)
    {
        ReverseMaps.Add(new PropertyMap<TDest, TSrc, TFieldDst, TFieldSrc>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }


    /// <summary>
    /// Adds a map defined by the asynchronous function<paramref name="customMap"/> from the source field selected by <paramref name="inputFieldSelector"/>
    /// to the destination field selected by <paramref name="outputFieldSelector"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source and destination property types are different and the mapping function is asynchronous.
    /// </remarks>
    /// <param name="inputFieldSelector"></param>
    /// <param name="outputFieldSelector"></param>
    /// <param name="customMap"></param>
    /// <typeparam name="TFieldSrc"></typeparam>
    /// <typeparam name="TFieldDst"></typeparam>
    /// <returns></returns>
    public MegaMapperMapBuilder<TSrc, TDest> MapField<TFieldSrc, TFieldDst>(Expression<Func<TSrc, TFieldSrc>> inputFieldSelector, Expression<Func<TDest, TFieldDst>> outputFieldSelector, Func<TSrc, TDest, TFieldSrc, Task<TFieldDst>> customMap)
    {
        Maps.Add(new PropertyMap<TSrc, TDest, TFieldSrc, TFieldDst>(ReflectionHelper.FindProperty(inputFieldSelector), ReflectionHelper.FindProperty(outputFieldSelector), customMap));
        return this;
    }

    /// <summary>
    /// Builds the mapping profile from the build map.
    /// </summary>
    /// <returns></returns>
    public IMegaMapperProfile BuildProfile()
    {
        return new MegaMapperInternalProfile<TSrc, TDest>(this);
    }
}