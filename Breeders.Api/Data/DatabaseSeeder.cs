using Breeders.Api.Entities;
using Breeders.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace Breeders.Api.Data;

public static class DatabaseSeeder
{
    public static readonly Guid TestBreederId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid SecondBreederId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid ApprovedLitterId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid DraftLitterId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid SubmittedLitterId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static readonly Guid OtherBreederLitterId =
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        if (await context.Litters.AnyAsync())
        {
            return;
        }

        var testBreederBenefit = new BreederBenefit
        {
            BreederId = TestBreederId,
            FreeLimit = 3,
            UsedCount = 0
        };

        var secondBreederBenefit = new BreederBenefit
        {
            BreederId = SecondBreederId,
            FreeLimit = 1,
            UsedCount = 1
        };

        var litters = new List<Litter>
        {
            new()
            {
                Id = ApprovedLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },

            new()
            {
                Id = DraftLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },

            new()
            {
                Id = SubmittedLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Submitted,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },

            new()
            {
                Id = OtherBreederLitterId,
                BreederId = SecondBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.BreederBenefits.AddRangeAsync(
            testBreederBenefit,
            secondBreederBenefit
        );

        await context.Litters.AddRangeAsync(litters);

        await context.SaveChangesAsync();
    }
}