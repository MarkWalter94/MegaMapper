namespace MegaMapper.Examples.Dto;

public class MultiDimension
{
    public Slot[][] SlotsMatrix { get; set; }
    public List<List<Slot>> SlotsMatrixList { get; set; }

    public int Id { get; set; }
}

public class Slot
{
    public int Id { get; set; }
}