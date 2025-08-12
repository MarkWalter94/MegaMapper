using System.Diagnostics;
using AutoMapper;
using MegaMapper;
using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using MegaMapper.PerformanceTest;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

var services = new ServiceCollection();

services.AddLogging();
services.AddMegaMapper();
services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = ""; // Put your license key for automapper
    cfg.CreateMap<UserComplex, UserComplexDto>()
        .ForMember(dest => dest.BirthYear,
            opt => opt.MapFrom(src => src.DateOfBirth.Year))
        .ReverseMap()
        .ForMember(dest => dest.DateOfBirth,
            opt => opt.MapFrom(src => new DateTime(src.BirthYear, 1, 1)));

    cfg.CreateMap<PerformanceClass, PerformanceClassDto>().ReverseMap();
    cfg.CreateMap<PerformanceChild, PerformanceChildDto>().ReverseMap();
});

services.AddMegaMapperBuilder<UserComplexToUserComplexDtoMapBuilder>();

var container = services.BuildServiceProvider();

var autoMapper = container.GetRequiredService<IMapper>();
var megaMapper = container.GetRequiredService<IMegaMapper>();


var user = new UserComplex
{
    Id = 1,
    FirstName = "Giulia",
    LastName = "Bianchi",
    DateOfBirth = new DateTime(1992, 3, 14)
};
var sw = new Stopwatch();

sw.Restart();
var dto = autoMapper.Map<UserComplexDto>(user);
var reversed = autoMapper.Map<UserComplex>(dto);
var elapsedMs = sw.ElapsedMilliseconds;
Console.WriteLine($"Automapper map in: {elapsedMs} mS");

Assert.Equal(user.Id, dto.Id);
Assert.Equal(user.FirstName, dto.FirstName);
Assert.Equal(user.LastName, dto.LastName);
Assert.Equal(user.DateOfBirth.Year, dto.BirthYear);
Assert.Equal(user.Id, reversed.Id);
Assert.Equal(user.FirstName, reversed.FirstName);
Assert.Equal(user.LastName, reversed.LastName);
Assert.Equal(new DateTime(dto.BirthYear, 1, 1), reversed.DateOfBirth);

sw.Restart();
dto = await megaMapper.Map<UserComplexDto>(user);
reversed = await megaMapper.Map<UserComplex>(dto);
elapsedMs = sw.ElapsedMilliseconds;
Console.WriteLine($"Megamapper map in: {elapsedMs} mS");


Assert.Equal(user.Id, dto.Id);
Assert.Equal(user.FirstName, dto.FirstName);
Assert.Equal(user.LastName, dto.LastName);
Assert.Equal(user.DateOfBirth.Year, dto.BirthYear);
Assert.Equal(user.Id, reversed.Id);
Assert.Equal(user.FirstName, reversed.FirstName);
Assert.Equal(user.LastName, reversed.LastName);
Assert.Equal(new DateTime(dto.BirthYear, 1, 1), reversed.DateOfBirth);


List<PerformanceClass> CreatePerformanceData(int count)
{
    var list = new List<PerformanceClass>(count);
    for (int i = 0; i < count; i++)
    {
        list.Add(new PerformanceClass
        {
            Name = $"Parent_{i}",
            Childs = new List<PerformanceChild>
            {
                new PerformanceChild
                {
                    Name = $"Child_{i}_1",
                    ConvertedDecimal = i + 0.1m
                },
                new PerformanceChild
                {
                    Name = $"Child_{i}_2",
                    ConvertedDecimal = i + 0.2m
                }
            }
        });
    }

    return list;
}

async Task TestPerformance(int count)
{
    var data = CreatePerformanceData(count);
    var sw = new Stopwatch();

    // AutoMapper
    sw.Restart();
    var dtoList = autoMapper.Map<List<PerformanceClassDto>>(data);
    var reversedList = autoMapper.Map<List<PerformanceClass>>(dtoList);
    sw.Stop();
    Console.WriteLine($"AutoMapper - {count} items: {sw.ElapsedMilliseconds} ms");
    // Verifica integrità dei dati
    Assert.Equal(data[0].Name, dtoList[0].Name);
    Assert.Equal((int)data[0].Childs[0].ConvertedDecimal, dtoList[0].Childs[0].ConvertedDecimal);
    
    // MegaMapper
    sw.Restart();
    
    var dtoListMega = await megaMapper.Map<List<PerformanceClassDto>>(data);
    var reversedListMega = await megaMapper.Map<List<PerformanceClass>>(dtoListMega);
    sw.Stop();
    Console.WriteLine($"MegaMapper - {count} items: {sw.ElapsedMilliseconds} ms");
    
    // Verifica integrità dei dati
    Assert.Equal(data[0].Name, dtoList[0].Name);
    Assert.Equal((int)data[0].Childs[0].ConvertedDecimal, dtoList[0].Childs[0].ConvertedDecimal);
}

// Eseguiamo per 100, 1000 e 100000 righe
await TestPerformance(1);
await TestPerformance(100);
await TestPerformance(1000);
await TestPerformance(10000);
await TestPerformance(100000);
await TestPerformance(1000000);