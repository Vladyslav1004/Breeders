using System.Text.Json.Serialization;
using Breeders.Api.Data;
using Breeders.Api.Middleware;
using Breeders.Api.Services;
using Breeders.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Database:Name"] ?? "BreedersDatabase";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(databaseName);
});

builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();

builder.Services.AddScoped<ILitterService, LitterService>();

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedAsync(app);
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}