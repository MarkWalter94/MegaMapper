using MegaMapper.Examples.Dto;
using MegaMapper.Examples.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MegaMapper.Examples;

public class MultiDimensionTest
{
    private readonly ServiceProvider _provider;
    private readonly IMegaMapper _mapper;

    public MultiDimensionTest()
    {
        var services = new ServiceCollection();
        services.AddMegaMapper();

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMegaMapper>();
    }


    [Fact]
    public async Task MultiDimensionTestTest()
    {
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
        var dto = await _mapper.Map<MultiDimensionDto>(model);

        // Assert forward mapping
        Assert.NotNull(dto);
        Assert.Equal(model.Id, dto.Id);
        Assert.Equal(model.SlotsMatrix.Length, dto.SlotsMatrix.Length);
        for (int i = 0; i < model.SlotsMatrix.Length; i++)
        {
            Assert.Equal(model.SlotsMatrix[i].Length, dto.SlotsMatrix[i].Length);
            for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
            {
                Assert.Equal(model.SlotsMatrix[i][j].Id, dto.SlotsMatrix[i][j].Id);
            }
        }

        Assert.Equal(model.SlotsMatrixList.Count, dto.SlotsMatrixList.Count);
        for (int i = 0; i < model.SlotsMatrixList.Count; i++)
        {
            Assert.Equal(model.SlotsMatrixList[i].Count, dto.SlotsMatrixList[i].Count);
            for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
            {
                Assert.Equal(model.SlotsMatrixList[i][j].Id, dto.SlotsMatrixList[i][j].Id);
            }
        }

        // Reverse mapping
        var modelBack = await _mapper.Map<MultiDimension>(dto);

        // Assert reverse mapping
        Assert.NotNull(modelBack);
        Assert.Equal(model.Id, modelBack.Id);
        Assert.Equal(model.SlotsMatrix.Length, modelBack.SlotsMatrix.Length);
        for (int i = 0; i < model.SlotsMatrix.Length; i++)
        {
            Assert.Equal(model.SlotsMatrix[i].Length, modelBack.SlotsMatrix[i].Length);
            for (int j = 0; j < model.SlotsMatrix[i].Length; j++)
            {
                Assert.Equal(model.SlotsMatrix[i][j].Id, modelBack.SlotsMatrix[i][j].Id);
            }
        }

        Assert.Equal(model.SlotsMatrixList.Count, modelBack.SlotsMatrixList.Count);
        for (int i = 0; i < model.SlotsMatrixList.Count; i++)
        {
            Assert.Equal(model.SlotsMatrixList[i].Count, modelBack.SlotsMatrixList[i].Count);
            for (int j = 0; j < model.SlotsMatrixList[i].Count; j++)
            {
                Assert.Equal(model.SlotsMatrixList[i][j].Id, modelBack.SlotsMatrixList[i][j].Id);
            }
        }
    }
}