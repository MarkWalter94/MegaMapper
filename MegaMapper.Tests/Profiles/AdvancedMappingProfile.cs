using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Services;

namespace MegaMapper.Examples.Profiles;

public class AdvancedMappingProfile : MegaMapperProfile<UserComplex, UserComplexDto>
{
    private readonly ICustomService _customService;
    public AdvancedMappingProfile(ICustomService customService)
    {
        _customService = customService;
    }
    protected override async Task<UserComplexDto> Map(UserComplex input, UserComplexDto output)
    {
        output.FirstName = await _customService.GetTheData();
        return output;
    }

    protected override Task<UserComplex> MapBack(UserComplexDto input, UserComplex output)
    {
        return Task.FromResult(output);
    }
}