using System.Text.Json.Serialization;
using Breeders.Api.Data;
using Breeders.Api.Middleware;
using Breeders.Api.Services;
using Breeders.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("BreedersDatabase");
});

builder.Services.AddScoped<
    INotificationService,
    ConsoleNotificationService>();

builder.Services.AddScoped<
    ILitterService,
    LitterService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app);

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();