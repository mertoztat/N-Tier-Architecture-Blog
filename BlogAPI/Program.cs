using BlogAPI.Core.Services;
using BlogAPI.Data.Extensions;
using BlogAPI.Filters;
using BlogAPI.Middlewares;
using BlogAPI.Services;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using BlogAPI.Service.Extensions;
using BlogAPI.Service.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


namespace BlogAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Current user (from JWT claims): used by AppDbContext for audit fields and by services for ownership checks.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Converts AppExceptions (NotFound, Forbidden, BadRequest) into ProblemDetails responses.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        // Every error response has the same shape: a Turkish "title" per status code, the actual message
        // in "detail", and field errors in "errors" (validation only). Applies to Problem()/ValidationProblem(),
        // [ApiController] 400s, ValidationFilter and GlobalExceptionHandler alike.
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;
                problem.Title = problem is HttpValidationProblemDetails
                    ? "Bir veya daha fazla doğrulama hatası oluştu."
                    : problem.Status switch
                    {
                        StatusCodes.Status400BadRequest => "Geçersiz istek",
                        StatusCodes.Status401Unauthorized => "Yetkisiz erişim",
                        StatusCodes.Status403Forbidden => "Erişim reddedildi",
                        StatusCodes.Status404NotFound => "Bulunamadı",
                        StatusCodes.Status409Conflict => "Çakışma",
                        StatusCodes.Status500InternalServerError => "Sunucu hatası",
                        _ => problem.Title
                    };
            };
        });

        builder.Services.LoadDataLayer(builder.Configuration);
        builder.Services.LoadServiceLayer(builder.Configuration);
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
     

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // JwtBearerOptions are configured from JwtSettings (validated on start in LoadServiceLayer).
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        // CORS for the React app. AllowCredentials is required for the refresh token cookie,
        // which means origins must be listed explicitly (no wildcard).
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        // Max 5 article creations per minute per user (per IP if somehow unauthenticated).
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("ArticleCreate", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Çok fazla istek",
                    status = 429,
                    detail = "Çok sık makale oluşturuyorsunuz. Lütfen biraz bekleyip tekrar deneyin."
                }, token);
            };
        });

        builder.Services.AddControllers(options =>
        {
            // Runs FluentValidation validators for action arguments (see Filters/ValidationFilter.cs).
            options.Filters.Add<ValidationFilter>();
            // Validation lives in FluentValidation; don't let MVC add its own English
            // "field is required" errors just because a property is a non-nullable string.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste the access token returned by /api/auth/login.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
  
        var app = builder.Build();

        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        // Serves wwwroot/images/... (uploaded article photos).
        app.UseStaticFiles();

        app.UseCors("Frontend");

        app.UseAuthentication();
        app.UseAuthorization();
        // After authentication so the limiter can partition by user.
        app.UseRateLimiter();


        app.MapControllers();

        app.Run();
    }
}