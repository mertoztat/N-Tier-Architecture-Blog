using System.Globalization;
using System.Reflection;
using System.Text.Json;
using BlogAPI.Service.Helpers.Images;
using FluentValidation;
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
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IImageHelper, ImageHelper>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddAutoMapper(assembly);

        // FluentValidation: registers every AbstractValidator<T> in this project (see FluentValidations/).
        // They are executed by ValidationFilter in the API project.
        services.AddValidatorsFromAssembly(assembly);
        // Built-in messages in Turkish, e.g. "'Başlık' boş olmamalı."
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr");
        // Stop at the first failing rule of a property: an empty e-mail only reports "boş olmamalı",
        // not also "geçerli bir e-posta adresi değil".
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        // Error keys in camelCase ("title" instead of "Title") so they match the JSON field names.
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

        return services;
    }
}
