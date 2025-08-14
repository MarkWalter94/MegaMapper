using System.Diagnostics;
using System.Text;
using AutoMapper;
using MegaMapper;
using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using MegaMapper.PerformanceTest;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


var sw1 = new Stopwatch();
int i = 0;
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
    cfg.CreateMap<MultiDimension, MultiDimensionDto>().ReverseMap();
    cfg.CreateMap<Slot, SlotDto>().ReverseMap();
});

services.AddMegaMapperBuilder<UserComplexToUserComplexDtoMapBuilder>();

var container = services.BuildServiceProvider();

var autoMapper = container.GetRequiredService<IMapper>();
var megaMapper = container.GetRequiredService<IMegaMapper>();

// Arrange
var model = new MultiDimension
{
    Id = 42,
    SlotsMatrix = new Slot[2][]
    {
        new Slot[] { new Slot { Id = 1 }, new Slot { Id = 2 } },
        new Slot[] { new Slot { Id = 3 }, new Slot { Id = 4 } }
    },
    SlotsMatrixList = new List<List<Slot>>
    {
        new List<Slot> { new Slot { Id = 5 }, new Slot { Id = 6 } },
        new List<Slot> { new Slot { Id = 7 }, new Slot { Id = 8 } }
    }
};

// Act
sw1.Restart();
var dto = autoMapper.Map<MultiDimensionDto>(model);
sw1.Stop();
Console.WriteLine($"AutoMapper forward multiDim {sw1.ElapsedMilliseconds} mS");
// Assert forward mapping
Assert.NotNull(dto);
Assert.Equal(model.Id, dto.Id);
Assert.Equal(model.SlotsMatrix.Length, dto.SlotsMatrix.Length);
for (i = 0; i < model.SlotsMatrix.Length; i++)
{
    Assert.Equal(model.SlotsMatrix[i].Length, dto.SlotsMatrix[i].Length);
    for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
    {
        Assert.Equal(model.SlotsMatrix[i][j].Id, dto.SlotsMatrix[i][j].Id);
    }
}

Assert.Equal(model.SlotsMatrixList.Count, dto.SlotsMatrixList.Count);
for (i = 0; i < model.SlotsMatrixList.Count; i++)
{
    Assert.Equal(model.SlotsMatrixList[i].Count, dto.SlotsMatrixList[i].Count);
    for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
    {
        Assert.Equal(model.SlotsMatrixList[i][j].Id, dto.SlotsMatrixList[i][j].Id);
    }
}

// Reverse mapping
sw1.Restart();
var modelBack = autoMapper.Map<MultiDimension>(dto);
sw1.Stop();
Console.WriteLine($"AutoMapper back multiDim {sw1.ElapsedMilliseconds} mS");

// Assert reverse mapping
Assert.NotNull(modelBack);
Assert.Equal(model.Id, modelBack.Id);
Assert.Equal(model.SlotsMatrix.Length, modelBack.SlotsMatrix.Length);
for (i = 0; i < model.SlotsMatrix.Length; i++)
{
    Assert.Equal(model.SlotsMatrix[i].Length, modelBack.SlotsMatrix[i].Length);
    for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
    {
        Assert.Equal(model.SlotsMatrix[i][j].Id, modelBack.SlotsMatrix[i][j].Id);
    }
}

Assert.Equal(model.SlotsMatrixList.Count, modelBack.SlotsMatrixList.Count);
for (i = 0; i < model.SlotsMatrixList.Count; i++)
{
    Assert.Equal(model.SlotsMatrixList[i].Count, modelBack.SlotsMatrixList[i].Count);
    for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
    {
        Assert.Equal(model.SlotsMatrixList[i][j].Id, modelBack.SlotsMatrixList[i][j].Id);
    }
}


// Act
sw1.Restart();
dto = await megaMapper.Map<MultiDimensionDto>(model);
sw1.Stop();
Console.WriteLine($"MegaMapper forward multiDim {sw1.ElapsedMilliseconds} mS");
// Assert forward mapping
Assert.NotNull(dto);
Assert.Equal(model.Id, dto.Id);
Assert.Equal(model.SlotsMatrix.Length, dto.SlotsMatrix.Length);
for (i = 0; i < model.SlotsMatrix.Length; i++)
{
    Assert.Equal(model.SlotsMatrix[i].Length, dto.SlotsMatrix[i].Length);
    for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
    {
        Assert.Equal(model.SlotsMatrix[i][j].Id, dto.SlotsMatrix[i][j].Id);
    }
}

Assert.Equal(model.SlotsMatrixList.Count, dto.SlotsMatrixList.Count);
for (i = 0; i < model.SlotsMatrixList.Count; i++)
{
    Assert.Equal(model.SlotsMatrixList[i].Count, dto.SlotsMatrixList[i].Count);
    for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
    {
        Assert.Equal(model.SlotsMatrixList[i][j].Id, dto.SlotsMatrixList[i][j].Id);
    }
}

// Reverse mapping
sw1.Restart();
modelBack = await megaMapper.Map<MultiDimension>(dto);
sw1.Stop();
Console.WriteLine($"MegaMapper back multiDim {sw1.ElapsedMilliseconds} mS");

// Assert reverse mapping
Assert.NotNull(modelBack);
Assert.Equal(model.Id, modelBack.Id);
Assert.Equal(model.SlotsMatrix.Length, modelBack.SlotsMatrix.Length);
for (i = 0; i < model.SlotsMatrix.Length; i++)
{
    Assert.Equal(model.SlotsMatrix[i].Length, modelBack.SlotsMatrix[i].Length);
    for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
    {
        Assert.Equal(model.SlotsMatrix[i][j].Id, modelBack.SlotsMatrix[i][j].Id);
    }
}

Assert.Equal(model.SlotsMatrixList.Count, modelBack.SlotsMatrixList.Count);
for (i = 0; i < model.SlotsMatrixList.Count; i++)
{
    Assert.Equal(model.SlotsMatrixList[i].Count, modelBack.SlotsMatrixList[i].Count);
    for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
    {
        Assert.Equal(model.SlotsMatrixList[i][j].Id, modelBack.SlotsMatrixList[i][j].Id);
    }
}


var user = new UserComplex
{
    Id = 1,
    FirstName = "Giulia",
    LastName = "Bianchi",
    DateOfBirth = new DateTime(1992, 3, 14)
};
var sw = new Stopwatch();

sw.Restart();
var dtoc = autoMapper.Map<UserComplexDto>(user);
var reversed = autoMapper.Map<UserComplex>(dtoc);
sw.Stop();
Console.WriteLine($"Automapper map in: {sw1.ElapsedMilliseconds} mS");

Assert.Equal(user.Id, dtoc.Id);
Assert.Equal(user.FirstName, dtoc.FirstName);
Assert.Equal(user.LastName, dtoc.LastName);
Assert.Equal(user.DateOfBirth.Year, dtoc.BirthYear);
Assert.Equal(user.Id, reversed.Id);
Assert.Equal(user.FirstName, reversed.FirstName);
Assert.Equal(user.LastName, reversed.LastName);
Assert.Equal(new DateTime(dtoc.BirthYear, 1, 1), reversed.DateOfBirth);

sw.Restart();
dtoc = await megaMapper.Map<UserComplexDto>(user);
reversed = await megaMapper.Map<UserComplex>(dtoc);
sw.Stop();
Console.WriteLine($"Megamapper map in: {sw1.ElapsedMilliseconds} mS");


Assert.Equal(user.Id, dtoc.Id);
Assert.Equal(user.FirstName, dtoc.FirstName);
Assert.Equal(user.LastName, dtoc.LastName);
Assert.Equal(user.DateOfBirth.Year, dtoc.BirthYear);
Assert.Equal(user.Id, reversed.Id);
Assert.Equal(user.FirstName, reversed.FirstName);
Assert.Equal(user.LastName, reversed.LastName);
Assert.Equal(new DateTime(dtoc.BirthYear, 1, 1), reversed.DateOfBirth);


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


async Task<PerformanceResult> TestPerformance(int count)
{
    var data = CreatePerformanceData(count);
    var sw = new Stopwatch();

    var ret = new PerformanceResult() { Count = count };

    // AutoMapper
    sw.Restart();
    var dtoList = autoMapper.Map<List<PerformanceClassDto>>(data);
    var reversedList = autoMapper.Map<List<PerformanceClass>>(dtoList);
    sw.Stop();
    ret.AutoMapperUs = (long)sw.Elapsed.TotalMicroseconds;
    // Verify
    Assert.Equal(data[0].Name, dtoList[0].Name);
    Assert.Equal((int)data[0].Childs[0].ConvertedDecimal, dtoList[0].Childs[0].ConvertedDecimal);

    // MegaMapper
    sw.Restart();
    var dtoListMega = await megaMapper.Map<List<PerformanceClassDto>>(data);
    var reversedListMega = await megaMapper.Map<List<PerformanceClass>>(dtoListMega);
    sw.Stop();
    ret.MegaMapperUs = (long)sw.Elapsed.TotalMicroseconds;

    // Verify
    Assert.Equal(data[0].Name, dtoList[0].Name);
    Assert.Equal((int)data[0].Childs[0].ConvertedDecimal, dtoList[0].Childs[0].ConvertedDecimal);
    return ret;
}

// Execute the test
List<PerformanceResult> results = new();
i = 1;
while (i <= 10000)
{
    results.Add(await TestPerformance(i));
    if (i < 10)
        ++i;
    else
        i += 100;
}

foreach (var res in results)
{
    Console.WriteLine($"{res.Count}: Automapper in {res.AutoMapperUs} uS, mega in {res.MegaMapperUs} uS");
}

class PerformanceResult
{
    public int Count { get; set; }
    public long AutoMapperUs { get; set; }
    public long MegaMapperUs { get; set; }
}