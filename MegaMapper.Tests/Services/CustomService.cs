namespace MegaMapper.Examples.Services;

public interface ICustomService
{
    public Task<string> GetTheData();
}

public class CustomService : ICustomService
{
    public Task<string> GetTheData()
    {
        return Task.FromResult("The data");
    }
}