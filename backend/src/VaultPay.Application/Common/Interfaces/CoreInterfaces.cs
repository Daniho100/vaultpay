using VaultPay.Domain.Entities;

namespace VaultPay.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public interface IJwtTokenService
{
    (string Token, DateTime Expiration) GenerateToken(User user, IList<string> roles);
}

public interface IIdempotencyService
{
    Task<IdempotencyRecord?> GetRecordAsync(Guid userId, string operationType, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IdempotencyRecord> CreateRecordAsync(Guid userId, string operationType, string idempotencyKey, string requestFingerprint, CancellationToken cancellationToken = default);
    Task CompleteRecordAsync(Guid recordId, string responseData, int statusCode, CancellationToken cancellationToken = default);
    Task FailRecordAsync(Guid recordId, string errorData, int statusCode, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string entityType, string? entityId, string? details, string? ipAddress = null, CancellationToken cancellationToken = default);
}
