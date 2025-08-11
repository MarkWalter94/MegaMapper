using MegaMapper.Examples.Dto;

namespace MegaMapper.Examples.Profiles;

public class UserToUserDtoAsyncMapBuilder : MegaMapperProfile<User, UserDto>
{
    protected override async Task<UserDto> Map(User input, UserDto output)
    {
        output.Id = input.Id;
        await Task.Delay(5);
        output.FirstName = input.FirstName.ToUpperInvariant();
        return output;
    }

    protected override Task<User> MapBack(UserDto input, User output)
    {
        output.FirstName = input.FirstName;
        output.Id = input.Id;
        return  Task.FromResult(output);
    }
}