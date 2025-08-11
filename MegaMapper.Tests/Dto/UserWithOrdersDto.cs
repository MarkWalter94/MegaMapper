using MegaMapper.Examples.Dto;

namespace MegaMapper.Examples.Profiles;

public class UserWithOrdersDto
{
    public int Id { get; set; }
    public List<Order> Orders { get; set; }
}