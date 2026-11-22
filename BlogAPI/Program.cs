using BlogAPI.Data.Context;
using BlogAPI.Data.Extensions;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Extensions;
using Microsoft.AspNetCore.Identity;


namespace BlogAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.LoadDataLayer(builder.Configuration);
        builder.Services.LoadServiceLayer(builder.Configuration);
        builder.Services.AddSession();
        
        builder.Services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
            })
            .AddRoleManager<RoleManager<AppRole>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(config =>
        {
            config.LoginPath = new PathString("/Admin/Auth/Login");
            config.LogoutPath = new PathString("/Admin/Auth/Logout");
            config.Cookie = new CookieBuilder
            {
                Name = "BlogAPI.Identity.Cookie",
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                SecurePolicy = CookieSecurePolicy.Always
            };
            config.SlidingExpiration = true;
            config.ExpireTimeSpan = TimeSpan.FromDays(7);
            config.AccessDeniedPath = new PathString("/Admin/Auth/AccessDenied");
        });
        
        
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
     

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
  
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseSession();
        // Add authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}