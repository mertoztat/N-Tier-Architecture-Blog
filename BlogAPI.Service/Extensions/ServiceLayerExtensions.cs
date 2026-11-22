using System.Reflection;
using BlogAPI.Service.Options;
using BlogAPI.Service.Services.Abstractions;
using BlogAPI.Service.Services.Concretes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Service.Extensions;

public static class ServiceLayerExtensions
{
    public static IServiceCollection LoadServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        
        var assembly = Assembly.GetExecutingAssembly();
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddAutoMapper(assembly);

        return services;
    }
}
