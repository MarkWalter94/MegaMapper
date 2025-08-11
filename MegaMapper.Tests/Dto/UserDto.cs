namespace MegaMapper.Examples.Dto;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    public AddressDto Address { get; set; }
    public List<OrderDto> Orders { get; set; }
}