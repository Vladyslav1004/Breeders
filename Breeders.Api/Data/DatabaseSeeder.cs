using Microsoft.EntityFrameworkCore;

namespace Breeders.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        var logger = loggerFactory.CreateLogger("DatabaseSeeder");

        await context.Database.EnsureCreatedAsync();

        var databaseAlreadyContainsData =
            await context.BreederBenefits.AnyAsync() ||
            await context.Litters.AnyAsync();

        if (databaseAlreadyContainsData)
        {
            logger.LogInformation(
                "Seed data were not added because the database already contains data.");

            return;
        }

        var modeValue =
            app.Configuration["SeedData:Mode"]
            ?? SeedDataMode.Fixed.ToString();

        if (!Enum.TryParse<SeedDataMode>(
                modeValue,
                ignoreCase: true,
                out var mode))
        {
            throw new InvalidOperationException(
                $"Unknown seed data mode '{modeValue}'. " +
                "Allowed values: Fixed or Generated.");
        }

        var seedData = DevelopmentSeedData.Create(
            mode,
            DateTime.UtcNow);

        await context.BreederBenefits.AddRangeAsync(
            seedData.BreederBenefits);

        await context.Litters.AddRangeAsync(
            seedData.Litters);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Development seed data created in {Mode} mode.",
            mode);

        logger.LogInformation(
            "Primary breeder: {PrimaryBreederId}",
            seedData.PrimaryBreederId);

        logger.LogInformation(
            "Approved litter: {ApprovedLitterId}",
            seedData.ApprovedLitterId);

        logger.LogInformation(
            "Draft litter: {DraftLitterId}",
            seedData.DraftLitterId);

        logger.LogInformation(
            "Submitted litter: {SubmittedLitterId}",
            seedData.SubmittedLitterId);

        logger.LogInformation(
            "Second breeder: {SecondBreederId}",
            seedData.SecondBreederId);

        logger.LogInformation(
            "Second breeder approved litter: {OtherBreederLitterId}",
            seedData.OtherBreederLitterId);
    }
}