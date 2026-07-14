using Breeders.Api.Data;
using Breeders.Api.Exceptions;
using Breeders.Api.Models;
using Breeders.Api.Models.DTOs;
using Breeders.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Breeders.Api.Services;

public class LitterService : ILitterService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public LitterService(
        AppDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<PagedResponse<LitterResponse>> GetLittersAsync(
        Guid breederId,
        LitterQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(parameters);

        var query = _context.Litters
            .AsNoTracking()
            .Where(litter => litter.BreederId == breederId);

        if (parameters.Status.HasValue)
        {
            query = query.Where(
                litter => litter.Status == parameters.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(litter => litter.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(litter => new LitterResponse
            {
                Id = litter.Id,
                Status = litter.Status,
                CreatedAt = litter.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<LitterResponse>
        {
            Items = items,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)parameters.PageSize)
        };
    }

    public async Task<LitterResponse> PublishAsync(
    Guid litterId,
    Guid breederId,
    CancellationToken cancellationToken = default)
    {
        var litter = await _context.Litters
            .FirstOrDefaultAsync(
                litter => litter.Id == litterId,
                cancellationToken);

        if (litter is null)
        {
            throw new NotFoundException(
                "Litter was not found.");
        }

        if (litter.BreederId != breederId)
        {
            throw new ForbiddenException(
                "You cannot publish another breeder's litter.");
        }

        if (litter.Status != Enums.LitterStatus.Approved)
        {
            throw new ValidationException(
                "Only an approved litter can be published.");
        }

        var benefit = await _context.BreederBenefits
            .FirstOrDefaultAsync(
                benefit => benefit.BreederId == breederId,
                cancellationToken);

        if (benefit is null)
        {
            throw new NotFoundException(
                "Breeder benefit was not found.");
        }

        if (benefit.UsedCount >= benefit.FreeLimit)
        {
            var failedAuditLog = new AuditLog
            {
                EntityId = litter.Id,
                Action = "Publish attempt failed - limits exceeded",
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(failedAuditLog);

            await _context.SaveChangesAsync(cancellationToken);

            throw new ConflictException(
                "Free publication limit has been exceeded.");
        }

        benefit.UsedCount++;
        litter.Status = Enums.LitterStatus.Published;

        var successAuditLog = new AuditLog
        {
            EntityId = litter.Id,
            Action = "Published for free",
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(successAuditLog);

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.SendLitterPublishedEmailAsync(
            breederId,
            litter.Id,
            cancellationToken);

        return new LitterResponse
        {
            Id = litter.Id,
            Status = litter.Status,
            CreatedAt = litter.CreatedAt
        };
    }

    private static void ValidatePagination(
        LitterQueryParameters parameters)
    {
        if (parameters.PageNumber < 1)
        {
            throw new ValidationException(
                "Page number must be greater than zero.");
        }

        if (parameters.PageSize < 1 || parameters.PageSize > 100)
        {
            throw new ValidationException(
                "Page size must be between 1 and 100.");
        }
    }
}