using Microsoft.Extensions.DependencyInjection;

namespace MegaMapper;

public static class DIExtensions
{
    /// <summary>
    /// Add megamapper in the DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMegaMapper(this IServiceCollection services)
    {
        services.AddScoped<IMegaMapper, MegaMapper>();
        return services;
    }

    /// <summary>
    /// Add a custome profile for the mapping engine.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TProfile"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddMegaMapperProfile<TProfile>(this IServiceCollection services) where TProfile : class, IMegaMapperProfile
    {
        services.AddScoped<IMegaMapperProfile, TProfile>();
        return services;
    }
    
    /// <summary>
    /// Add a custom builder for the mapping engine.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddMegaMapperBuilder<TBuilder>(this IServiceCollection services) where TBuilder : class, IMegaMapperMapBuilder
    {
        services.AddScoped<IMegaMapperMapBuilder, TBuilder>();
        return services;
    }
}