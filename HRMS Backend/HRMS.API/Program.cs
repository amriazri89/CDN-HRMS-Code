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
builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== MEDIATR - Registers all Commands/Queries/Handlers ==========
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(HRMS.Application.AssemblyReference.Assembly);
});

// ========== FLUENTVALIDATION - Registers all Validators ==========
builder.Services.AddValidatorsFromAssembly(HRMS.Application.AssemblyReference.Assembly);

// ========== VALIDATION BEHAVIOR - Runs validators before handlers ==========
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ========== DAPPER REPOSITORIES ==========
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmploymentRecordRepository, EmploymentRecordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ========== SERVICES ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// JWT Configuration
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "https://etiqa.assessment.vercel.app"  ,
            "https://etiqaassessment.vercel.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
var app = builder.Build();

app.UseCors("AllowViteApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();