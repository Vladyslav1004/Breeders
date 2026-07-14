using Breeders.Api.Data;
using Breeders.Api.Enums;

namespace Breeders.Tests.Data;

public class DevelopmentSeedDataTests
{
    [Fact]
    public void Create_WhenModeIsFixed_UsesKnownIds()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;

        // Act
        var result = DevelopmentSeedData.Create(
            SeedDataMode.Fixed,
            currentTime);

        // Assert
        Assert.Equal(
            DevelopmentSeedData.FixedIds.PrimaryBreederId,
            result.PrimaryBreederId);

        Assert.Equal(
            DevelopmentSeedData.FixedIds.ApprovedLitterId,
            result.ApprovedLitterId);
    }

    [Fact]
    public void Create_WhenModeIsGenerated_CreatesNonEmptyUniqueIds()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;

        // Act
        var result = DevelopmentSeedData.Create(
            SeedDataMode.Generated,
            currentTime);

        var ids = new[]
        {
            result.PrimaryBreederId,
            result.SecondBreederId,
            result.ApprovedLitterId,
            result.DraftLitterId,
            result.SubmittedLitterId,
            result.OtherBreederLitterId
        };

        // Assert
        Assert.DoesNotContain(Guid.Empty, ids);

        Assert.Equal(
            ids.Length,
            ids.Distinct().Count());
    }

    [Fact]
    public void Create_CreatesApprovedLitterForPrimaryBreeder()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;

        // Act
        var result = DevelopmentSeedData.Create(
            SeedDataMode.Generated,
            currentTime);

        var approvedLitter = result.Litters.Single(
            litter => litter.Id == result.ApprovedLitterId);

        // Assert
        Assert.Equal(
            result.PrimaryBreederId,
            approvedLitter.BreederId);

        Assert.Equal(
            LitterStatus.Approved,
            approvedLitter.Status);
    }

    [Fact]
    public void Create_CreatesSecondBreederWithExceededLimit()
    {
        // Arrange
        var result = DevelopmentSeedData.Create(
            SeedDataMode.Generated,
            DateTime.UtcNow);

        // Act
        var secondBreederBenefit =
            result.BreederBenefits.Single(
                benefit =>
                    benefit.BreederId ==
                    result.SecondBreederId);

        // Assert
        Assert.Equal(
            secondBreederBenefit.FreeLimit,
            secondBreederBenefit.UsedCount);
    }
}