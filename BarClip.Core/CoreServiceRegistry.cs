using Azure.Identity;
using Azure.Storage.Blobs;
using BarClipApi.Core.Repositories;
using BarClipApi.Core.Services;
using BarClipApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace BarClipApi.Core;

public static class CoreServiceRegistry
{
    public static IServiceCollection RegisterCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database registration
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );
                sqlOptions.UseRelationalNulls();
            }));



        // Repository registration
        RegisterRepositories(services);

        // Service registration
        RegisterServices(services);

        RegisterExternalServices(services);

        return services;
    }
    public static IServiceCollection RegisterFunctionServices(this IServiceCollection services)
    {

        services.AddScoped<FunctionService>();
        services.AddScoped<StorageService>();

        RegisterExternalServices(services);
        services.AddSingleton<IApiClientService, ApiClientService>();


        return services;
    }
    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<VideoRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<SessionRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<StorageService>();
        services.AddScoped<SessionService>();
    }
    private static void RegisterExternalServices(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration["AzureWebJobsStorage"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Storage connection string is not configured.");
            }

            return new BlobServiceClient(connectionString);
        });
    }

}
