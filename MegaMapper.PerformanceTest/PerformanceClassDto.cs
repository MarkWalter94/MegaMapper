namespace MegaMapper.PerformanceTest;

public class PerformanceClassDto
{
    public string Name { get; set; }
    public List<PerformanceChildDto> Childs { get; set; }
}

public class PerformanceChildDto
{
    public string Name { get; set; }
    public int ConvertedDecimal { get; set; }
}