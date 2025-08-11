namespace MegaMapper.Examples.Profiles;

public class UserWithOrdersToDtoMapBuilder : MegaMapperMapBuilder<UserWithOrders, UserWithOrdersDto>
{
    public UserWithOrdersToDtoMapBuilder()
    {
        AutoMapField(s => s.Id, d => d.Id);
        AutoMapField(s => s.Orders, d => d.Orders);
    }
}