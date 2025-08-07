using System.Diagnostics;

namespace MegaMapper;

/// <summary>
/// Interface defining a generic mapping profile between two types.
/// </summary>
public interface IMegaMapperProfile
{
    /// <summary>
    /// Returns the type of the input type.
    /// </summary>
    /// <returns></returns>
    Type GetTIn();
    
    /// <summary>
    /// Returns the type of the output ty
    /// </summary>
    /// <returns></returns>
    Type GetTOut();
    
    /// <summary>
    /// Maps the object.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<object> MapInternal(object input);
    
    /// <summary>
    /// Maps the object.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    Task<object> MapInternal(object input, object output);
    
    /// <summary>
    /// Reverse maps the object.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<object> MapBackInternal(object input);

    /// <summary>
    /// Performs reverse mapping from the output object to an existing input object.
    /// </summary>
    /// <param name="input">The output object.</param>
    /// <param name="output">The existing input object to populate.</param>
    /// <returns>The mapped input object.</returns>
    Task<object> MapBackInternal(object input, object output);
}

/// <summary>
/// Abstract base class that implements IMegaMapperProfile for mapping between two specific types.
/// </summary>
/// <typeparam name="TIn">The input type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
public abstract class MegaMapperProfile<TIn, TOut> : IMegaMapperProfile
{
    private readonly Type _tin = typeof(TIn), _tout = typeof(TOut);

    /// <inheritdoc />
    public Type GetTIn()
    {
        return _tin;
    }

    /// <inheritdoc />
    public Type GetTOut()
    {
        return _tout;
    }

    /// <inheritdoc />
    public async Task<object> MapInternal(object input)
    {
        var output = Activator.CreateInstance<TOut>();
        Debug.Assert(output != null);

        return await MapInternal(input, output);
    }

    /// <inheritdoc />
    public async Task<object> MapInternal(object input, object output)
    {
        return (await Map((TIn)input, (TOut)output))!;
    }

    /// <inheritdoc />
    public async Task<object> MapBackInternal(object input)
    {
        var output = Activator.CreateInstance<TIn>();
        Debug.Assert(output != null);

        return await MapBackInternal(input, output);
    }

    /// <inheritdoc />
    public async Task<object> MapBackInternal(object input, object output)
    {
        return (await MapBack((TOut)input, (TIn)output))!;
    }

    /// <summary>
    /// Mapping function from TIn to TOut.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    protected abstract Task<TOut> Map(TIn input, TOut output);

    /// <summary>
    /// Inverse mapping function from TOut to TIn.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    protected abstract Task<TIn> MapBack(TOut input, TIn output);
}