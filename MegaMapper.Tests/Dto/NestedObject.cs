namespace MegaMapper.Examples.Dto;

public class NestedObject
{
    public int Id { get; set; }
    public NestedObjectLiv1 NestedLiv1 { get; set; }
    public NestedObjectListLiv1 NestedListLiv1 {get; set; }
}

public class NestedObjectLiv1
{
    public int Id { get; set; }
    public NestedObjectLiv2 NestedLiv2 { get; set; }
}

public class NestedObjectLiv2
{
    public int Id { get; set; }
    public NestedObjectLiv1 NestedUpLiv1 { get; set; }
}

public class NestedObjectListLiv1
{
    public int Id { get; set; }
    public List<NestedObjectLiv2> NestedListLiv2 { get; set; }
}
public class NestedObjectListLiv2
{
    public int Id { get; set; }
    public List<NestedObjectListLiv1> NestedListUpLiv1 { get; set; }
}