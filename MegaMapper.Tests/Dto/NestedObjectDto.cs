namespace MegaMapper.Examples.Dto;

public class NestedObjectDto
{
    public int Id { get; set; }
    public NestedObjectLiv1Dto NestedLiv1 { get; set; }
    public NestedObjectListLiv1Dto NestedListLiv1 {get; set; }
}

public class NestedObjectLiv1Dto
{
    public int Id { get; set; }
    public NestedObjectLiv2Dto NestedLiv2 { get; set; }
}

public class NestedObjectLiv2Dto
{
    public int Id { get; set; }
    public NestedObjectLiv1Dto NestedUpLiv1 { get; set; }
}

public class NestedObjectListLiv1Dto
{
    public int Id { get; set; }
    public List<NestedObjectLiv2Dto> NestedListLiv2 { get; set; }
}
public class NestedObjectListLiv2Dto
{
    public int Id { get; set; }
    public List<NestedObjectListLiv1Dto> NestedListUpLiv1 { get; set; }
}