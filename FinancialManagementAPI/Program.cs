using FinancialManagementAPI.Database;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Financial Management API",
        Version = "v1",
        Description = "API para gerenciamento financeiro",
        Contact = new OpenApiContact {
            Name = "Luís Victor Belo",
            Email = "luis.victor.belo@gmail.com",
            Url = new Uri("https://github.com/luisvictorbelo/FinancialManagementAPI")
        }
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Habilita o Swagger no app
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial Management API v1");
        c.RoutePrefix = "";
    });
    app.MapOpenApi();
}

// HTTP -> HTTPS
app.UseHttpsRedirection();
// Habilita Controllers
app.UseAuthorization();
app.MapControllers();
app.Run();

