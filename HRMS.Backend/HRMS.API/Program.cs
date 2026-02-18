using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using HRMS.Application.Behaviors;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== KESTREL CONFIG ==========
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
    Console.WriteLine("⚡ Development mode → HTTP only on port 5000");
}
else
{
    // Production: Use HTTPS with self-signed cert
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5000); // ← add this for Vercel
        options.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.UseHttps("C:\\certs\\api.pfx", "hrms");
        });
    });
    Console.WriteLine("🔒 Production mode → HTTP:5000 + HTTPS:5001")
}

// ========== SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== MEDIATR ==========
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(HRMS.Application.AssemblyReference.Assembly);
});

// ========== FLUENTVALIDATION ==========
builder.Services.AddValidatorsFromAssembly(HRMS.Application.AssemblyReference.Assembly);

// ========== VALIDATION BEHAVIOR ==========
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ========== CACHING ==========
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// ========== REPOSITORIES ========== 
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();// ✅ Caching is disabled for now to ensure data consistency during development and testing

//this is cahing make crud effect more longer
//builder.Services.AddScoped<IEmployeeRepository>(sp =>
//    new CachedEmployeeRepository(
//        sp.GetRequiredService<EmployeeRepository>(),
//        sp.GetRequiredService<ICacheService>(),
//        sp.GetRequiredService<ILogger<CachedEmployeeRepository>>()
//    ));

/*
When caching makes sense:

-High traffic apps with thousands of users
-Data that rarely changes (e.g. company settings, roles)
-Expensive queries that take long to run

When it doesn't make sense (your case):

-Small app with few users
-Data changes frequently (CRUD operations)
-The bug it causes is worse than the performance gain */

builder.Services.AddScoped<IEmploymentRecordRepository, EmploymentRecordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ========== SERVICES ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// ========== JWT ==========
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HRMSApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HRMSClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                new Uri(origin).Host.EndsWith("vercel.app") ||
                new Uri(origin).Host == "localhost" ||
                new Uri(origin).Host == "127.0.0.1"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Pagination")
            .AllowCredentials();
    });
});

// ========== BUILD ==========
var app = builder.Build();

app.MapGet("/", () => "HRMS API is running...");

// ✅ CORS MUST come first
app.UseCors("AllowVercel");

// Swagger for dev only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }
