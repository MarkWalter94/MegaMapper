namespace MegaMapper.Examples.Dto;

public class MultiDimensionDto
{
    public SlotDto[][] SlotsMatrix { get; set; }
    public List<List<SlotDto>> SlotsMatrixList { get; set; }

    public int Id { get; set; }
}

public class SlotDto
{
    public int Id { get; set; }
}