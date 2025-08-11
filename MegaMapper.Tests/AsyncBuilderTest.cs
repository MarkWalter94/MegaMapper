using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using MegaMapper.Examples.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MegaMapper.Examples;

public class AsyncBuilderTest
{
    private readonly ServiceProvider _provider;
    private readonly IMegaMapper _mapper;
    private readonly ICustomService _customService;

    public AsyncBuilderTest()
    {
        var services = new ServiceCollection();
        services.AddMegaMapper();

        //Custom service that give us the data..
        services.AddScoped<ICustomService, CustomService>();
        
        //Multiple maps for the mapping..
        //Order matters, maps are applied in order!
        //If any of the maps has the property UseBaseMap, then first the base map will be applied.
        services.AddMegaMapperBuilder<UserComplexToUserComplexDtoMapBuilder>();
        services.AddMegaMapperBuilder<AdvancedMappingBuilder>();

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMegaMapper>();
        _customService = _provider.GetRequiredService<ICustomService>();
    }

    [Fact]
    public async Task AdvancedMappingBuilderTest()
    {
        var user = new UserComplex
        {
            Id = 1,
            FirstName = "Giulia",
            LastName = "Bianchi",
            DateOfBirth = new DateTime(1992, 3, 14)
        };

        var dto = await _mapper.Map<UserComplexDto>(user);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(await _customService.GetTheData(), dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.DateOfBirth.Year, dto.BirthYear);

        var reversed = await _mapper.Map<UserComplex>(dto);
        Assert.Equal(user.Id, reversed.Id);
        Assert.Equal(await _customService.GetTheData(), reversed.FirstName);
        Assert.Equal(user.LastName, reversed.LastName);
        Assert.Equal(new DateTime(dto.BirthYear, 1, 1), reversed.DateOfBirth);
    }
}