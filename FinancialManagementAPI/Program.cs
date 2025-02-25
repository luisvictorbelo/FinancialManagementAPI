using System.Text;
using FinancialManagementAPI.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Inicialização DbContext
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
    + "'Default Connection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Adiciona  os Controllers
builder.Services.AddControllers();

//Configuração Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial Management API",
        Version = "v1",
        Description = "API para gerenciamento financeiro",
        Contact = new OpenApiContact
        {
            Name = "Luís Victor Belo",
            Email = "luis.victor.belo@gmail.com",
            Url = new Uri("https://github.com/luisvictorbelo/FinancialManagementAPI")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        {
            new OpenApiSecurityScheme 
            {
                Reference = new OpenApiReference 
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Autenticação JWT
var key = builder.Configuration.GetSection("Jwt").ToString() ?? "";
if (string.IsNullOrEmpty(key)) throw new NullReferenceException(nameof(key));

var encodedKey = Encoding.UTF8.GetBytes(key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(encodedKey)
        };
    });

builder.Services.AddAuthorization();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Habilita o Swagger no app
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial Management API v1");
        c.RoutePrefix = "";
    });
    app.MapOpenApi();
}

// HTTP -> HTTPS
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Habilita Controllers
app.MapControllers();

app.Run();

