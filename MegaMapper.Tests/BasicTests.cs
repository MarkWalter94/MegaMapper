using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MegaMapper.Examples;

public class BasicTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IMegaMapper _mapper;

    public BasicTests()
    {
        var services = new ServiceCollection();
        services.AddMegaMapper();

        // Registra tutti i builder/profile usati nei test
        services.AddMegaMapperBuilder<UserComplexToUserComplexDtoMapBuilder>();
        services.AddMegaMapperProfile<UserToUserDtoAsyncMapBuilder>();
        services.AddMegaMapperBuilder<UserToUserDtoTypeConversionMapBuilder>();
        services.AddMegaMapperBuilder<UserWithAddressToDtoMapBuilder>();
        services.AddMegaMapperBuilder<UserWithOrdersToDtoMapBuilder>();

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMegaMapper>();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
    
    [Fact]
    public async Task Should_Map_User_To_UserDto_And_Back()
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
        Assert.Equal(user.FirstName, dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.DateOfBirth.Year, dto.BirthYear);

        var reversed = await _mapper.Map<UserComplex>(dto);
        Assert.Equal(user.Id, reversed.Id);
        Assert.Equal(user.FirstName, reversed.FirstName);
        Assert.Equal(user.LastName, reversed.LastName);
        Assert.Equal(new DateTime(dto.BirthYear, 1, 1), reversed.DateOfBirth);
    }

    [Fact]
    public async Task Should_Map_Using_Async_Function()
    {
        var user = new User { Id = 1, FirstName = "giulia" };

        var dto = await _mapper.Map<UserDto>(user);

        Assert.Equal("GIULIA", dto.FirstName);
    }

    [Fact]
    public async Task Should_Map_With_Type_Conversion()
    {
        var user = new UserConversion { Id = 42 };

        var dto = await _mapper.Map<UserDtoConversion>(user);

        Assert.Equal("User-42", dto.FirstName);

        var reversed = await _mapper.Map<UserConversion>(dto);

        Assert.Equal(42, reversed.Id);
    }

    [Fact]
    public async Task Should_Map_Nested_Properties()
    {
        var user = new UserWithAddress
        {
            Id = 1,
            Address = new Address { Street = "Via Dante", City = "Torino" }
        };

        var dto = await _mapper.Map<UserWithAddressDto>(user);

        Assert.Equal("Via Dante", dto.Street);
        Assert.Equal("Torino", dto.City);
    }

    [Fact]
    public async Task Should_Map_Collections()
    {
        var user = new UserWithOrders
        {
            Id = 1,
            Orders = new List<Order>
            {
                new Order { OrderId = 1, TotalAmount = 100 },
                new Order { OrderId = 2, TotalAmount = 200 }
            }
        };

        var dto = await _mapper.Map<UserWithOrdersDto>(user);

        Assert.Equal(2, dto.Orders.Count);
        Assert.Equal(200, dto.Orders[1].TotalAmount);
    }
}