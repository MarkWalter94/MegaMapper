using MegaMapper.Examples.Dto;

namespace MegaMapper.Examples.Profiles;

public class UserComplexToUserComplexDtoMapBuilder : MegaMapperMapBuilder<UserComplex, UserComplexDto>
{
    public UserComplexToUserComplexDtoMapBuilder()
    {
        // Mappatura diretta
        AutoMapField(s => s.Id, d => d.Id);
        AutoMapField(s => s.FirstName, d => d.FirstName);
        AutoMapField(s => s.LastName, d => d.LastName);

        // Mappatura custom
        MapField(s => s.DateOfBirth, d => d.BirthYear,
            (src, dest, val) => val.Year);

        // Reverse map custom
        MapFieldBack(d => d.BirthYear, s => s.DateOfBirth,
            (src, dest, val) => new DateTime(val, 1, 1));
    }
}