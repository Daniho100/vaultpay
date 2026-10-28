using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;

namespace VaultPay.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IApplicationDbContext _context;

    public IdempotencyService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyRecord?> GetRecordAsync(
        Guid userId, 
        string operationType, 
        string idempotencyKey, 
        CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .FirstOrDefaultAsync(r => 
                r.UserId == userId && 
                r.OperationType == operationType && 
                r.IdempotencyKey == idempotencyKey, 
                cancellationToken);
    }

    public async Task<IdempotencyRecord> CreateRecordAsync(
        Guid userId, 
        string operationType, 
        string idempotencyKey, 
        string requestFingerprint, 
        CancellationToken cancellationToken = default)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OperationType = operationType,
            IdempotencyKey = idempotencyKey,
            RequestFingerprint = requestFingerprint,
            Status = IdempotencyStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _context.IdempotencyRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task CompleteRecordAsync(
        Guid recordId, 
        string responseData, 
        int statusCode, 
        CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotencyRecords.FirstOrDefaultAsync(r => r.Id == recordId, cancellationToken);
        if (record != null)
        {
            record.Status = IdempotencyStatus.Completed;
            record.ResponseData = responseData;
            record.ResponseStatusCode = statusCode;
            record.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task FailRecordAsync(
        Guid recordId, 
        string errorData, 
        int statusCode, 
        CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotencyRecords.FirstOrDefaultAsync(r => r.Id == recordId, cancellationToken);
        if (record != null)
        {
            record.Status = IdempotencyStatus.Failed;
            record.ResponseData = errorData;
            record.ResponseStatusCode = statusCode;
            record.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
