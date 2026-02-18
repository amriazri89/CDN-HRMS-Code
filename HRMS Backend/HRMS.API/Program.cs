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
    Console.WriteLine("🔒 Production mode → reading Kestrel config from appsettings.Production.json");
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

// ========== CACHING (No Docker / No Redis needed!) ==========
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// ========== DAPPER REPOSITORIES ==========
// Register real EmployeeRepository first (concrete class)
builder.Services.AddScoped<EmployeeRepository>();

// Then wrap it with CachedEmployeeRepository (Decorator Pattern)
builder.Services.AddScoped<IEmployeeRepository>(sp =>
    new CachedEmployeeRepository(
        sp.GetRequiredService<EmployeeRepository>(),
        sp.GetRequiredService<ICacheService>(),
        sp.GetRequiredService<ILogger<CachedEmployeeRepository>>()
    ));

// Other repositories (no caching needed for these)
builder.Services.AddScoped<IEmploymentRecordRepository, EmploymentRecordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ========== SERVICES ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// ========== JWT CONFIGURATION ==========
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
                new Uri(origin).Host.EndsWith("vercel.app"))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// ========== BUILD ==========
var app = builder.Build();

app.MapGet("/", () => "HRMS API is running...");
app.UseCors("AllowVercel");

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