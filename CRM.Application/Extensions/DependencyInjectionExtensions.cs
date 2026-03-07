using Crm.Infrastructure.Services;
using CRM.Application.Common.Pagination;
using CRM.Application.Services.Auth_Service;
using CRM.Application.Services.Brand_Service;
using CRM.Application.Services.Menu_Permission_Service;
using CRM.Application.Services.ModuleMenu_Service;
using CRM.Application.Services.UserModule_Serves;
using CRM.Application.Services.Work_Context;
using CRM.Application.Services.Product_Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CRM.Application.Services.ProductCategory_Services;
using CRM.Application.Services.ProductSubCategory_Service;
using CRM.Application.Services.Banner_Service;
using CRM.Application.Services.HomeCategory_Service;
using CRM.Application.Services.ContactInfo_Service;
using CRM.Application.Services.ContactMessage_Service;
using CRM.Application.Services.Order_Service;
using CRM.Application.Services.Vendor_Service;
using CRM.Application.Services.Email_Service;

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
        services.AddScoped<IBrandService, BrandServices>();
        services.AddScoped<IProductCategoryServices, ProductCategoryServices>();
        services.AddScoped<IProductSubCategoryService, ProductSubCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<IHomeCategoryService, HomeCategoryService>();
        services.AddScoped<IContactInfoService, ContactInfoService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IVendorService, VendorService>();
        
        // Email Service Configuration
        services.Configure<EmailSettings>(config.GetSection("Email"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
