namespace MegaMapper.PerformanceTest;

public class PerformanceClass
{
    public string Name { get; set; }
     public List<PerformanceChild> Childs { get; set; }
}

public class PerformanceChild
{
    public string Name { get; set; }
    public decimal ConvertedDecimal { get; set; }
}