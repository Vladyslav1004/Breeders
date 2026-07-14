using Breeders.Api.Enums;
using Breeders.Api.Models;

namespace Breeders.Api.Data;

public static class DevelopmentSeedData
{
    public static class FixedIds
    {
        public static readonly Guid PrimaryBreederId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111111111");

        public static readonly Guid SecondBreederId =
            Guid.Parse(
                "22222222-2222-2222-2222-222222222222");

        public static readonly Guid ApprovedLitterId =
            Guid.Parse(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        public static readonly Guid DraftLitterId =
            Guid.Parse(
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        public static readonly Guid SubmittedLitterId =
            Guid.Parse(
                "cccccccc-cccc-cccc-cccc-cccccccccccc");

        public static readonly Guid OtherBreederLitterId =
            Guid.Parse(
                "dddddddd-dddd-dddd-dddd-dddddddddddd");
    }

    public static SeedDataSet Create(SeedDataMode mode, DateTime currentUtcTime)
    {
        return mode switch
        {
            SeedDataMode.Fixed => CreateWithIds(
                FixedIds.PrimaryBreederId,
                FixedIds.SecondBreederId,
                FixedIds.ApprovedLitterId,
                FixedIds.DraftLitterId,
                FixedIds.SubmittedLitterId,
                FixedIds.OtherBreederLitterId,
                currentUtcTime),

            SeedDataMode.Generated => CreateWithIds(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                currentUtcTime),

            _ => throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Unsupported seed data mode.")
        };
    }

    private static SeedDataSet CreateWithIds(
        Guid primaryBreederId,
        Guid secondBreederId,
        Guid approvedLitterId,
        Guid draftLitterId,
        Guid submittedLitterId,
        Guid otherBreederLitterId,
        DateTime currentUtcTime)
    {
        var benefits = new List<BreederBenefit>
        {
            new()
            {
                BreederId = primaryBreederId,
                FreeLimit = 3,
                UsedCount = 0
            },

            new()
            {
                BreederId = secondBreederId,
                FreeLimit = 1,
                UsedCount = 1
            }
        };

        var litters = new List<Litter>
        {
            new()
            {
                Id = approvedLitterId,
                BreederId = primaryBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = currentUtcTime.AddDays(-3)
            },

            new()
            {
                Id = draftLitterId,
                BreederId = primaryBreederId,
                Status = LitterStatus.Draft,
                CreatedAt = currentUtcTime.AddDays(-2)
            },

            new()
            {
                Id = submittedLitterId,
                BreederId = primaryBreederId,
                Status = LitterStatus.Submitted,
                CreatedAt = currentUtcTime.AddDays(-1)
            },

            new()
            {
                Id = otherBreederLitterId,
                BreederId = secondBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = currentUtcTime
            }
        };

        return new SeedDataSet
        {
            PrimaryBreederId = primaryBreederId,
            SecondBreederId = secondBreederId,
            ApprovedLitterId = approvedLitterId,
            DraftLitterId = draftLitterId,
            SubmittedLitterId = submittedLitterId,
            OtherBreederLitterId = otherBreederLitterId,
            BreederBenefits = benefits,
            Litters = litters
        };
    }
}