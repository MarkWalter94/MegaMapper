using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Services;
using Xunit.Sdk;

namespace MegaMapper.Examples.Profiles;

public class AdvancedMappingProfile : MegaMapperMapBuilder<UserComplex, UserComplexDto>
{
    private readonly ICustomService _customService;

    //Every property is map with the base core mapping, then overrided with the properties defined here.
    public override bool UseBaseMap => true;

    public AdvancedMappingProfile(ICustomService customService)
    {
        _customService = customService;

        MapField<string, string>(x => x.FirstName, y => y.FirstName, async (a, b, c) => await _customService.GetTheData());
    }
}