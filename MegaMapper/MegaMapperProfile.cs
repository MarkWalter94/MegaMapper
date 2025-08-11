using System.Diagnostics;

namespace MegaMapper;

public interface IMegaMapperProfile
{
    Type GetTIn();
    Type GetTOut();
    Task<object> MapInternal(object input);
    Task<object> MapInternal(object input, object output);
    Task<object> MapBackInternal(object input);
    Task<object> MapBackInternal(object input, object output);
}

public abstract class MegaMapperProfile<TIn, TOut> : IMegaMapperProfile
{
    private readonly Type _tin = typeof(TIn), _tout = typeof(TOut);

    public Type GetTIn()
    {
        return _tin;
    }

    public Type GetTOut()
    {
        return _tout;
    }

    public async Task<object> MapInternal(object input)
    {
        var output = Activator.CreateInstance<TOut>();
        Debug.Assert(output != null);

        return await MapInternal(input, output);
    }

    public async Task<object> MapInternal(object input, object output)
    {
        return (await Map((TIn)input, (TOut)output))!;
    }

    public async Task<object> MapBackInternal(object input)
    {
        var output = Activator.CreateInstance<TIn>();
        Debug.Assert(output != null);

        return await MapBackInternal(input, output);
    }

    public async Task<object> MapBackInternal(object input, object output)
    {
        return (await MapBack((TOut)input, (TIn)output))!;
    }

    /// <summary>
    /// Funzione di mapping da TIn a TOut.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    protected abstract Task<TOut> Map(TIn input, TOut output);

    /// <summary>
    /// Funzione di mapping inversa da TOut a TIn.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    protected abstract Task<TIn> MapBack(TOut input, TIn output);
}