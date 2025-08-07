using System.Diagnostics;
using System.Reflection;

namespace MegaMapper;

/// <summary>
/// Interface for mapping a single property or field from a source object to a destination object.
/// </summary>
/// <typeparam name="TSrc">The source type.</typeparam>
/// <typeparam name="TDest">The destination type.</typeparam>
public interface IPropertyMap<in TSrc, in TDest>
{
    /// <summary>
    /// Executes the property mapping from the source object to the destination object.
    /// </summary>
    /// <param name="src">The source object.</param>
    /// <param name="dest">The destination object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Map(TSrc src, TDest dest);

    /// <summary>
    /// A textual description of the mapping, usually indicating which property is mapped to which.
    /// </summary>
    string Description { get; }
}

/// <summary>
/// Implementation of a property mapping between a source and destination type, supporting both synchronous and asynchronous mapping.
/// </summary>
/// <typeparam name="TSrc">The source type.</typeparam>
/// <typeparam name="TDest">The destination type.</typeparam>
/// <typeparam name="TFieldSrc">The type of the source field or property.</typeparam>
/// <typeparam name="TFieldDest">The type of the destination field or property.</typeparam>
public class PropertyMap<TSrc, TDest, TFieldSrc, TFieldDest> : IPropertyMap<TSrc, TDest>
{
    /// <summary>
    /// The synchronous mapping function, if provided.
    /// </summary>
    private Func<TSrc, TDest, TFieldSrc, TFieldDest>? Map { get; }

    /// <summary>
    /// The asynchronous mapping function, if provided.
    /// </summary>
    private Func<TSrc, TDest, TFieldSrc, Task<TFieldDest>>? AsyncMap { get; }

    /// <summary>
    /// The source member (property or field).
    /// </summary>
    private MemberInfo SrcMember { get; }

    /// <summary>
    /// The destination member (property or field).
    /// </summary>
    private MemberInfo DestMember { get; }

    /// <summary>
    /// Initializes the base mapping structure with source and destination member info.
    /// </summary>
    /// <param name="srcMember">The source member.</param>
    /// <param name="destMember">The destination member.</param>
    private PropertyMap(MemberInfo srcMember, MemberInfo destMember)
    {
        ArgumentNullException.ThrowIfNull(srcMember);
        ArgumentNullException.ThrowIfNull(destMember);
        
        SrcMember = srcMember;
        DestMember = destMember;
        
        Description = $"{srcMember.Name} => {destMember.Name}";
    }

    /// <summary>
    /// Initializes a property map with a synchronous mapping function.
    /// </summary>
    /// <param name="sourceMember">The source member.</param>
    /// <param name="destMember">The destination member.</param>
    /// <param name="map">The synchronous mapping function.</param>
    public PropertyMap(MemberInfo sourceMember, MemberInfo destMember, Func<TSrc, TDest, TFieldSrc, TFieldDest> map) : this(sourceMember, destMember)
    {
        ArgumentNullException.ThrowIfNull(map);
        
        Map = map;
        AsyncMap = null;
    }

    /// <summary>
    /// Initializes a property map with an asynchronous mapping function.
    /// </summary>
    /// <param name="srcMember">The source member.</param>
    /// <param name="destMember">The destination member.</param>
    /// <param name="asyncMap">The asynchronous mapping function.</param>
    public PropertyMap(MemberInfo srcMember, MemberInfo destMember, Func<TSrc, TDest, TFieldSrc, Task<TFieldDest>> asyncMap) : this(srcMember, destMember)
    {
        ArgumentNullException.ThrowIfNull(asyncMap);

        AsyncMap = asyncMap;
        Map = null;
    }


    /// <summary>
    /// Does the mapping things.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    async Task IPropertyMap<TSrc, TDest>.Map(TSrc src, TDest dest)
    {
        Debug.Assert(src != null && dest != null);
        object? res;
        if (AsyncMap != null)
        {
            res = await AsyncMap(src, dest, (TFieldSrc)this.SrcMember.GetMemberValue(src)!);
        }
        else
        {
            Debug.Assert(Map != null);
            res = Map(src, dest, (TFieldSrc)this.SrcMember.GetMemberValue(src)!);
        }

        this.DestMember.SetMemberValue(dest, res);
    }

    public string Description { get; }
}