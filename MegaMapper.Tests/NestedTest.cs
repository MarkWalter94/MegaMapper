using MegaMapper.Examples.Dto;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MegaMapper.Examples;

public class NestedTest
{
    private readonly ServiceProvider _provider;
    private readonly IMegaMapper _mapper;

    public NestedTest()
    {
        var services = new ServiceCollection();
        services.AddMegaMapper();

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMegaMapper>();
    }


    [Fact]
    public async Task NestedTestTest()
    {
        // Livelli principali
        var root = new NestedObject { Id = 1 };
        var liv1 = new NestedObjectLiv1 { Id = 2 };
        var liv2 = new NestedObjectLiv2 { Id = 3 };
        var listLiv1 = new NestedObjectListLiv1 { Id = 4 };
        var listLiv2 = new NestedObjectListLiv2 { Id = 5 };

        // Collegamenti base
        root.NestedLiv1 = liv1;
        root.NestedListLiv1 = listLiv1;

        liv1.NestedLiv2 = liv2;
        liv2.NestedUpLiv1 = liv1; // ciclo diretto

        // Liste
        listLiv1.NestedListLiv2 = new List<NestedObjectLiv2> { liv2, new NestedObjectLiv2 { Id = 6 } };
        listLiv2.NestedListUpLiv1 = new List<NestedObjectListLiv1> { listLiv1 };

        // Opzionale: Aggiungere un collegamento incrociato nelle liste per aumentare il casino
        listLiv1.NestedListLiv2.Add(new NestedObjectLiv2
        {
            Id = 7,
            NestedUpLiv1 = new NestedObjectLiv1
            {
                Id = 8,
                NestedLiv2 = liv2 // riutilizzo di oggetto già esistente
            }
        });

        var dto = await _mapper.Map<NestedObjectDto>(root);
        
        // Assert principali
        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);

        Assert.NotNull(dto.NestedLiv1);
        Assert.Equal(2, dto.NestedLiv1.Id);

        Assert.NotNull(dto.NestedLiv1.NestedLiv2);
        Assert.Equal(3, dto.NestedLiv1.NestedLiv2.Id);

        // Verifica ciclo diretto
        Assert.NotNull(dto.NestedLiv1.NestedLiv2.NestedUpLiv1);
        Assert.Equal(2, dto.NestedLiv1.NestedLiv2.NestedUpLiv1.Id);

        // Verifica lista principale
        Assert.NotNull(dto.NestedListLiv1);
        Assert.Equal(4, dto.NestedListLiv1.Id);

        Assert.NotNull(dto.NestedListLiv1.NestedListLiv2);
        Assert.Equal(3, dto.NestedListLiv1.NestedListLiv2.First().Id); // primo elemento della lista

        // Verifica elemento aggiuntivo con collegamento incrociato
        var lastItem = dto.NestedListLiv1.NestedListLiv2.Last();
        Assert.Equal(7, lastItem.Id);
        Assert.NotNull(lastItem.NestedUpLiv1);
        Assert.Equal(8, lastItem.NestedUpLiv1.Id);

        // Verifica che il riutilizzo di liv2 abbia mantenuto lo stesso Id
        Assert.Equal(3, lastItem.NestedUpLiv1.NestedLiv2.Id);
    }
}