using FinalPPE.Data;
using FinalPPE.Models;
using FinalPPE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "FinalPPE";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            }
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }
            ];
            operation.Responses ??= new OpenApiResponses();
            operation.Responses["401"] = new OpenApiResponse { Description = "Token ausente o inválido" };
        }
        return Task.CompletedTask;
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Configura Jwt:Key con dotnet user-secrets.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("La clave JWT debe tener al menos 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

builder.Services.AddScoped<
    ITrofeosService,
    TrofeosService
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>
>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("FinalPPE"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("VueClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();