using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MegaMapper.Examples;

public class SimpleMap
{
    [Fact]
    public async Task Map()
    {
        var user = new User
        {
            Id = 1,
            FirstName = "Giulia",
            LastName = "Bianchi",
            DateOfBirth = new DateTime(1992, 3, 14),
            Address = new Address
            {
                Street = "Via Dante 45",
                City = "Torino",
                ZipCode = "10100"
            },
            Orders = new List<Order>
            {
                new Order { OrderId = 2001, OrderDate = new DateTime(2025, 7, 1), TotalAmount = 200m },
                new Order { OrderId = 2002, OrderDate = new DateTime(2025, 8, 5), TotalAmount = 350.5m }
            }
        };

        // Act
        var svcCollection = new ServiceCollection();
        svcCollection.AddMegaMapper();
        await using var container = svcCollection.BuildServiceProvider();
        await using var scope = container.CreateAsyncScope();

        var mapper = scope.ServiceProvider.GetRequiredService<IMegaMapper>();
        var dto = await mapper.Map<UserDto>(user);

        Assert.NotNull(dto);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.FirstName, dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.DateOfBirth, dto.DateOfBirth);

        Assert.NotNull(dto.Address);
        Assert.Equal(user.Address.Street, dto.Address.Street);
        Assert.Equal(user.Address.City, dto.Address.City);
        Assert.Equal(user.Address.ZipCode, dto.Address.ZipCode);

        Assert.NotNull(dto.Orders);
        Assert.Equal(user.Orders.Count, dto.Orders.Count);

        for (int i = 0; i < user.Orders.Count; i++)
        {
            Assert.Equal(user.Orders[i].OrderId, dto.Orders[i].OrderId);
            Assert.Equal(user.Orders[i].OrderDate, dto.Orders[i].OrderDate);
            Assert.Equal(user.Orders[i].TotalAmount, dto.Orders[i].TotalAmount);
        }
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

        var services = new ServiceCollection();
        services.AddMegaMapper();
        services.AddMegaMapperBuilder<UserComplexToUserComplexDtoMapBuilder>();

        await using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMegaMapper>();

        // Forward mapping
        var dto = await mapper.Map<UserComplexDto>(user);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.FirstName, dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.DateOfBirth.Year, dto.BirthYear);

        // Reverse mapping
        var reversed = await mapper.Map<UserComplex>(dto);
        Assert.Equal(user.Id, reversed.Id);
        Assert.Equal(user.FirstName, reversed.FirstName);
        Assert.Equal(user.LastName, reversed.LastName);
        Assert.Equal(new DateTime(dto.BirthYear, 1, 1), reversed.DateOfBirth);
    }
    
    [Fact]
    public async Task Should_Map_Using_Async_Function()
    {
        var user = new User { Id = 1, FirstName = "giulia" };

        var services = new ServiceCollection();
        services.AddMegaMapper();
        services.AddMegaMapperProfile<UserToUserDtoAsyncMapBuilder>();

        await using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMegaMapper>();

        var dto = await mapper.Map<UserDto>(user);
        Assert.Equal("GIULIA", dto.FirstName);
    }
    
    [Fact]
    public async Task Should_Map_With_Type_Conversion()
    {
        var user = new User { Id = 42 };

        var services = new ServiceCollection();
        services.AddMegaMapper();
        services.AddMegaMapperBuilder<UserToUserDtoTypeConversionMapBuilder>();

        await using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMegaMapper>();

        var dto = await mapper.Map<UserDto>(user);
        Assert.Equal("User-42", dto.FirstName);

        var reversed = await mapper.Map<User>(dto);
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

        var services = new ServiceCollection();
        services.AddMegaMapper();
        services.AddMegaMapperBuilder<UserWithAddressToDtoMapBuilder>();

        await using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMegaMapper>();

        var dto = await mapper.Map<UserWithAddressDto>(user);
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

        var services = new ServiceCollection();
        services.AddMegaMapper();
        services.AddMegaMapperBuilder<UserWithOrdersToDtoMapBuilder>();

        await using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMegaMapper>();

        var dto = await mapper.Map<UserWithOrdersDto>(user);
        Assert.Equal(2, dto.Orders.Count);
        Assert.Equal(200, dto.Orders[1].TotalAmount);
    }
}


    
