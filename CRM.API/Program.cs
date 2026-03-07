using CRM.Domain.Entities.Auth;
using CRM.Infrastructure;
using CRM.Infrastructure.Repositories;
using CRM.Application.Extensions;
using CRM.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CRM.WebAPI.Extensions;
using CRM.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);


// ======================== SERVICES ========================

// ✅ Controllers
builder.Services.AddControllers();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "CRM Web API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ✅ Database
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ✅ Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<CrmDbContext>()
.AddDefaultTokenProviders();


// ✅ JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new Exception("JWT Key must be at least 32 characters.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set TRUE in production
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        ClockSkew = TimeSpan.Zero
    };
});


// ✅ Application Services (Clean Architecture)
builder.Services.ConfigureServices(builder.Configuration);

// ✅ Infrastructure: Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ✅ API-layer Services (MediaService etc.)
builder.Services.AddApiServices();


var app = builder.Build();


// ======================== MIDDLEWARE ========================


// ✅ Always enable Swagger (helps avoid confusion)
app.UseSwagger();
app.UseSwaggerUI();


// ✅ Global Exception Handler
app.UseMiddleware<GlobalExceptionHandler>();


// ✅ HTTPS
app.UseHttpsRedirection();


// ✅ CORS BEFORE auth
app.UseCors("AllowAll");


// ✅ Authentication → Authorization
app.UseAuthentication();
app.UseAuthorization();


// ✅ Map Controllers (THIS prevents 404)
app.MapControllers();


// OPTIONAL: Root endpoint to avoid 404 on "/"
app.MapGet("/", () => "CRM API is running...");


app.Run();
