namespace MegaMapper.Examples.Dto;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Address Address { get; set; }
    public List<Order> Orders { get; set; }
}
