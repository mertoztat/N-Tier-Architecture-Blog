using System.Reflection;
using BlogAPI.Service.Services.Abstractions;
using BlogAPI.Service.Services.Concretes;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Service.Extensions;

public static class ServiceLayerExtensions
{
    public static IServiceCollection LoadServiceLayer(this IServiceCollection services)
    {
        
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddAutoMapper(assembly);

        return services;
    }
}
