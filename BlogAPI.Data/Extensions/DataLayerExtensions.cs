using BlogAPI.Data.Context;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;
using BlogAPI.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Data.Extensions;

public static class DataLayerExtensions
{
    public static IServiceCollection LoadDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Register the AppDbContext with the connection string from configuration move this to the Program.cs file
        // it is better to keep the DbContext registration in the Program.cs file for better separation of concerns and easier configuration management.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        //  Dependency Injection for UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}