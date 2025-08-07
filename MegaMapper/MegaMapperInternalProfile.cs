using System.Diagnostics;

namespace MegaMapper;

internal class MegaMapperInternalProfile<TSrc, TDest> : MegaMapperProfile<TSrc, TDest>
{
    private readonly MegaMapperMapBuilder<TSrc, TDest> _builder;

    internal MegaMapperInternalProfile(MegaMapperMapBuilder<TSrc, TDest> builder)
    {
        _builder = builder;
        Debug.Assert(_builder.Maps.Count != 0 || _builder.ReverseMaps.Count != 0);
    }

    protected override async Task<TDest> Map(TSrc input, TDest output)
    {
        Debug.Assert(input != null && output != null);

        foreach (var map in _builder.Maps)
        {
            try
            {
                await map.Map(input, output);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error mapping {typeof(TSrc).FullName} to {typeof(TDest).FullName}, map: {map.Description}", ex);
            }
        }

        return output;
    }

    protected override async Task<TSrc> MapBack(TDest input, TSrc output)
    {
        Debug.Assert(input != null && output != null);

        foreach (var map in _builder.ReverseMaps)
        {
            try
            {
                await map.Map(input, output);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error mapping {typeof(TSrc).FullName} to {typeof(TDest).FullName}, map: {map.Description}", ex);
            }
        }

        return output;
    }
}