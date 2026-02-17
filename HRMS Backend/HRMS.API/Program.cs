using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using HRMS.Application.Behaviors;
using HRMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== KESTREL CONFIG ==========
// ✅ FIX: Do NOT put Kestrel config here if using appsettings.json
// Let appsettings.Development.json / appsettings.Production.json handle it
// Program.cs only forces HTTP in Development as safety net

if (builder.Environment.IsDevelopment())
{
    // Force HTTP only locally — overrides anything in appsettings
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
    Console.WriteLine("⚡ Development mode → HTTP only on port 5000");
}
else
{
    // Production → read from appsettings.Production.json (Kestrel section)
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

// ========== DAPPER REPOSITORIES ==========
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
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
    options.AddPolicy("AllowViteApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "https://etiqaassessment.vercel.app",
            "https://cdnhrms-ten.vercel.app",
            "https://*.vercel.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// ========== BUILD ==========
var app = builder.Build();

app.MapGet("/", () => "HRMS API is running...");
app.UseCors("AllowViteApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // ✅ Only redirect to HTTPS in production
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();