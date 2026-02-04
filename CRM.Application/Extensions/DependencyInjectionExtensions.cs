using Crm.Infrastructure.Services;
using CRM.Application.Common.Pagination;
using CRM.Application.Services.Auth_Service;
using CRM.Application.Services.Menu_Permission_Service;
using CRM.Application.Services.ModuleMenu_Service;
using CRM.Application.Services.UserModule_Serves;
using CRM.Application.Services.Work_Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Application.Extensions;
public static class DependencyInjectionExtensions
{

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)

    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IWorkContext, WorkContextsService>();
        services.AddScoped<IMenuPermissionService, MenuPermissionService>();
        services.AddScoped<IRolesService, RolesService>();

        services.AddScoped<IUserModuleService, UserModuleService>();
        services.AddScoped<IModuleMenuService, ModuleMenuService>();

        services.AddScoped<IPaginationService, PaginationService>();

        return services;
    }
}
