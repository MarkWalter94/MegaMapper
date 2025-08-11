using MegaMapper.Examples.Dto;

namespace MegaMapper.Examples.Profiles;

public class UserToUserDtoTypeConversionMapBuilder : MegaMapperMapBuilder<UserConversion, UserDtoConversion>
{
    public UserToUserDtoTypeConversionMapBuilder()
    {
        MapField(s => s.Id, d => d.FirstName,
            (src, dest, val) => $"User-{val}");

        MapFieldBack(d => d.FirstName, s => s.Id,
            (dest, src, val) => int.Parse(val.Split('-')[1]));
    }
}
