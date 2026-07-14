using Microsoft.EntityFrameworkCore;

namespace Breeders.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Seed data додаються лише тоді, коли відповідні
        // таблиці ще не містять записів.
        // Це захищає програму від повторного додавання
        // однакових тестових даних.

        if (!await context.BreederBenefits.AnyAsync())
        {
            var benefits =
                DevelopmentSeedData.CreateBreederBenefits();

            await context.BreederBenefits.AddRangeAsync(benefits);
        }

        if (!await context.Litters.AnyAsync())
        {
            var litters = DevelopmentSeedData.CreateLitters(
                DateTime.UtcNow);

            await context.Litters.AddRangeAsync(litters);
        }

        await context.SaveChangesAsync();
    }
}