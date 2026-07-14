using Breeders.Api.Enums;
using Breeders.Api.Models;

namespace Breeders.Api.Data;

public static class DevelopmentSeedData
{
    // Фіксовані ідентифікатори використовуються для того,
    // щоб дані були передбачуваними після кожного запуску.
    // Завдяки цьому перевіряльник може копіювати ці ID
    // у Swagger без пошуку випадково створених GUID.

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

    // Перший заводчик використовується для перевірки
    // успішної безкоштовної публікації.
    //
    // Другий заводчик уже використав увесь ліміт,
    // тому він потрібен для перевірки помилки limits exceeded.

    public static IReadOnlyCollection<BreederBenefit>
        CreateBreederBenefits()
    {
        return new List<BreederBenefit>
        {
            new()
            {
                BreederId = TestBreederId,
                FreeLimit = 3,
                UsedCount = 0
            },

            new()
            {
                BreederId = SecondBreederId,
                FreeLimit = 1,
                UsedCount = 1
            }
        };
    }

    // Виводки з різними статусами дозволяють перевірити:
    // 1. Успішну публікацію Approved.
    // 2. Помилку для Draft.
    // 3. Помилку для Submitted.
    // 4. Перевірку власника.
    // 5. Перевищення безкоштовного ліміту.

    public static IReadOnlyCollection<Litter> CreateLitters(
        DateTime currentUtcTime)
    {
        return new List<Litter>
        {
            new()
            {
                Id = ApprovedLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = currentUtcTime.AddDays(-3)
            },

            new()
            {
                Id = DraftLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Draft,
                CreatedAt = currentUtcTime.AddDays(-2)
            },

            new()
            {
                Id = SubmittedLitterId,
                BreederId = TestBreederId,
                Status = LitterStatus.Submitted,
                CreatedAt = currentUtcTime.AddDays(-1)
            },

            new()
            {
                Id = OtherBreederLitterId,
                BreederId = SecondBreederId,
                Status = LitterStatus.Approved,
                CreatedAt = currentUtcTime
            }
        };
    }
}