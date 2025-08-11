namespace MegaMapper.Examples.Profiles;

public class UserWithAddressToDtoMapBuilder : MegaMapperMapBuilder<UserWithAddress, UserWithAddressDto>
{
    public UserWithAddressToDtoMapBuilder()
    {
        AutoMapField(s => s.Id, d => d.Id);
        MapField(s => s.Address, d => d.Street, (src, dest, addr) => addr?.Street ?? string.Empty);
        MapField(s => s.Address, d => d.City, (src, dest, addr) => addr?.City ?? string.Empty);
    }
}