using BlogAPI.Data.Context;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Entities;
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

        // Identity: AddIdentityCore is used instead of AddIdentity because the API authenticates
        // with JWT and does not need Identity's cookie schemes or SignInManager.
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}