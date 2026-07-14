using Breeders.Api.Data;
using Breeders.Api.Models;
using Breeders.Api.Models.DTOs;
using Breeders.Api.Enums;
using Breeders.Api.Exceptions;
using Breeders.Api.Services;
using Breeders.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Breeders.Tests.Services;

public class LitterServiceTests
{
    [Fact]
    public async Task PublishAsync_WhenDataIsValid_PublishesLitter()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();
        var litterId = Guid.NewGuid();

        var litter = new Litter
        {
            Id = litterId,
            BreederId = breederId,
            Status = LitterStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        var benefit = new BreederBenefit
        {
            BreederId = breederId,
            FreeLimit = 3,
            UsedCount = 0
        };

        context.Litters.Add(litter);
        context.BreederBenefits.Add(benefit);

        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var result = await service.PublishAsync(
            litterId,
            breederId);

        // Assert
        Assert.Equal(litterId, result.Id);
        Assert.Equal(LitterStatus.Published, result.Status);

        var savedLitter = await context.Litters
            .SingleAsync(x => x.Id == litterId);

        Assert.Equal(
            LitterStatus.Published,
            savedLitter.Status);

        var savedBenefit = await context.BreederBenefits
            .SingleAsync(x => x.BreederId == breederId);

        Assert.Equal(1, savedBenefit.UsedCount);

        var auditLog = await context.AuditLogs.SingleAsync();

        Assert.Equal(litterId, auditLog.EntityId);
        Assert.Equal("Published for free", auditLog.Action);

        Assert.True(notificationService.WasCalled);
        Assert.Equal(
            breederId,
            notificationService.LastBreederId);

        Assert.Equal(
            litterId,
            notificationService.LastLitterId);
    }

    [Fact]
    public async Task PublishAsync_WhenLitterDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.PublishAsync(
                Guid.NewGuid(),
                Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "Litter was not found.",
            exception.Message);

        Assert.Empty(context.AuditLogs);
        Assert.False(notificationService.WasCalled);
    }

    [Fact]
    public async Task PublishAsync_WhenBreederIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        await using var context = CreateContext();

        var ownerId = Guid.NewGuid();
        var anotherBreederId = Guid.NewGuid();

        var litter = new Litter
        {
            Id = Guid.NewGuid(),
            BreederId = ownerId,
            Status = LitterStatus.Approved
        };

        context.Litters.Add(litter);
        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => service.PublishAsync(
                litter.Id,
                anotherBreederId));

        // Assert
        Assert.Equal(
            "You cannot publish another breeder's litter.",
            exception.Message);

        Assert.Equal(LitterStatus.Approved, litter.Status);
        Assert.Empty(context.AuditLogs);
        Assert.False(notificationService.WasCalled);
    }

    [Theory]
    [InlineData(LitterStatus.Draft)]
    [InlineData(LitterStatus.Submitted)]
    [InlineData(LitterStatus.Published)]
    public async Task PublishAsync_WhenStatusIsNotApproved_ThrowsValidationException(
        LitterStatus status)
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();

        var litter = new Litter
        {
            Id = Guid.NewGuid(),
            BreederId = breederId,
            Status = status
        };

        var benefit = new BreederBenefit
        {
            BreederId = breederId,
            FreeLimit = 3,
            UsedCount = 0
        };

        context.Litters.Add(litter);
        context.BreederBenefits.Add(benefit);

        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.PublishAsync(
                litter.Id,
                breederId));

        // Assert
        Assert.Equal(
            "Only an approved litter can be published.",
            exception.Message);

        Assert.Equal(status, litter.Status);
        Assert.Equal(0, benefit.UsedCount);
        Assert.Empty(context.AuditLogs);
        Assert.False(notificationService.WasCalled);
    }

    [Fact]
    public async Task PublishAsync_WhenBenefitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();

        var litter = new Litter
        {
            Id = Guid.NewGuid(),
            BreederId = breederId,
            Status = LitterStatus.Approved
        };

        context.Litters.Add(litter);
        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.PublishAsync(
                litter.Id,
                breederId));

        // Assert
        Assert.Equal(
            "Breeder benefit was not found.",
            exception.Message);

        Assert.Equal(LitterStatus.Approved, litter.Status);
        Assert.Empty(context.AuditLogs);
        Assert.False(notificationService.WasCalled);
    }

    [Fact]
    public async Task PublishAsync_WhenLimitIsExceeded_CreatesFailedAuditLog()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();

        var litter = new Litter
        {
            Id = Guid.NewGuid(),
            BreederId = breederId,
            Status = LitterStatus.Approved
        };

        var benefit = new BreederBenefit
        {
            BreederId = breederId,
            FreeLimit = 3,
            UsedCount = 3
        };

        context.Litters.Add(litter);
        context.BreederBenefits.Add(benefit);

        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            out var notificationService);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.PublishAsync(
                litter.Id,
                breederId));

        // Assert
        Assert.Equal(
            "Free publication limit has been exceeded.",
            exception.Message);

        Assert.Equal(LitterStatus.Approved, litter.Status);
        Assert.Equal(3, benefit.UsedCount);

        var auditLog = await context.AuditLogs.SingleAsync();

        Assert.Equal(litter.Id, auditLog.EntityId);
        Assert.Equal(
            "Publish attempt failed - limits exceeded",
            auditLog.Action);

        Assert.False(notificationService.WasCalled);
    }

    [Fact]
    public async Task GetLittersAsync_ReturnsOnlyCurrentBreederLitters()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();
        var anotherBreederId = Guid.NewGuid();

        var firstLitter = CreateLitter(
            breederId,
            LitterStatus.Draft,
            DateTime.UtcNow.AddDays(-2));

        var secondLitter = CreateLitter(
            breederId,
            LitterStatus.Approved,
            DateTime.UtcNow.AddDays(-1));

        var anotherBreederLitter = CreateLitter(
            anotherBreederId,
            LitterStatus.Approved,
            DateTime.UtcNow);

        context.Litters.AddRange(
            firstLitter,
            secondLitter,
            anotherBreederLitter);

        await context.SaveChangesAsync();

        var service = CreateService(context, out _);

        // Act
        var result = await service.GetLittersAsync(
            breederId,
            new LitterQueryParameters());

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        Assert.Contains(
            result.Items,
            item => item.Id == firstLitter.Id);

        Assert.Contains(
            result.Items,
            item => item.Id == secondLitter.Id);

        Assert.DoesNotContain(
            result.Items,
            item => item.Id == anotherBreederLitter.Id);
    }

    [Fact]
    public async Task GetLittersAsync_WhenStatusProvided_FiltersByStatus()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();

        var approvedLitter = CreateLitter(
            breederId,
            LitterStatus.Approved,
            DateTime.UtcNow);

        var draftLitter = CreateLitter(
            breederId,
            LitterStatus.Draft,
            DateTime.UtcNow.AddDays(-1));

        context.Litters.AddRange(
            approvedLitter,
            draftLitter);

        await context.SaveChangesAsync();

        var service = CreateService(context, out _);

        var parameters = new LitterQueryParameters
        {
            Status = LitterStatus.Approved
        };

        // Act
        var result = await service.GetLittersAsync(
            breederId,
            parameters);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        Assert.Equal(
            approvedLitter.Id,
            result.Items.Single().Id);

        Assert.Equal(
            LitterStatus.Approved,
            result.Items.Single().Status);
    }

    [Fact]
    public async Task GetLittersAsync_AppliesPaginationAndOrdering()
    {
        // Arrange
        await using var context = CreateContext();

        var breederId = Guid.NewGuid();

        var baseDate = new DateTime(
            2026,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var litters = Enumerable.Range(1, 5)
            .Select(number => CreateLitter(
                breederId,
                LitterStatus.Approved,
                baseDate.AddMinutes(number)))
            .ToList();

        context.Litters.AddRange(litters);
        await context.SaveChangesAsync();

        var service = CreateService(context, out _);

        var parameters = new LitterQueryParameters
        {
            PageNumber = 2,
            PageSize = 2
        };

        // Act
        var result = await service.GetLittersAsync(
            breederId,
            parameters);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);

        var resultIds = result.Items
            .Select(item => item.Id)
            .ToArray();

        var expectedIds = new[]
        {
            litters[2].Id,
            litters[1].Id
        };

        Assert.Equal(expectedIds, resultIds);
    }

    [Fact]
    public async Task GetLittersAsync_WhenNoLittersExist_ReturnsEmptyPage()
    {
        // Arrange
        await using var context = CreateContext();

        var service = CreateService(context, out _);

        // Act
        var result = await service.GetLittersAsync(
            Guid.NewGuid(),
            new LitterQueryParameters());

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetLittersAsync_WhenPageNumberIsInvalid_ThrowsValidationException()
    {
        // Arrange
        await using var context = CreateContext();

        var service = CreateService(context, out _);

        var parameters = new LitterQueryParameters
        {
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetLittersAsync(
                Guid.NewGuid(),
                parameters));

        // Assert
        Assert.Equal(
            "Page number must be greater than zero.",
            exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetLittersAsync_WhenPageSizeIsInvalid_ThrowsValidationException(
        int pageSize)
    {
        // Arrange
        await using var context = CreateContext();

        var service = CreateService(context, out _);

        var parameters = new LitterQueryParameters
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetLittersAsync(
                Guid.NewGuid(),
                parameters));

        // Assert
        Assert.Equal(
            "Page size must be between 1 and 100.",
            exception.Message);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(
                $"BreedersTests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static LitterService CreateService(
        AppDbContext context,
        out FakeNotificationService notificationService)
    {
        notificationService = new FakeNotificationService();

        return new LitterService(
            context,
            notificationService);
    }

    private static Litter CreateLitter(
        Guid breederId,
        LitterStatus status,
        DateTime createdAt)
    {
        return new Litter
        {
            Id = Guid.NewGuid(),
            BreederId = breederId,
            Status = status,
            CreatedAt = createdAt
        };
    }
}