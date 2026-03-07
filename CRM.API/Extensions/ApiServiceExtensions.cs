using app.WebApp.Handlers;
using CRM.Application.Interfaces.Medias;

namespace CRM.WebAPI.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddTransient<IMediaService, MediaService>();
        return services;
    }
}
