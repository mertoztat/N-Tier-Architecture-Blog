using BlogAPI.Data.Extensions;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;
using BlogAPI.Service.Extensions;


namespace BlogAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.LoadDataLayer(builder.Configuration);
        builder.Services.LoadServiceLayer(builder.Configuration);
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

        // Add authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}