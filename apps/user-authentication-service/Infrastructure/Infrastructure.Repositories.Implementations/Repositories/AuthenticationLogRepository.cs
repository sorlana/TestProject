using Domain.Entities.Entities;
using Domain.Entities.Enums;
using Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Services.Repositories.Abstractions.Interfaces;

namespace Infrastructure.Repositories.Implementations.Repositories;

/// <summary>
/// Реализация репозитория логов аутентификации
/// Обеспечивает доступ к данным логов операций аутентификации через Entity Framework
/// </summary>
public class AuthenticationLogRepository : IAuthenticationLogRepository
{
    private readonly DatabaseContext _context;

    public AuthenticationLogRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task CreateAsync(AuthenticationLog log, CancellationToken cancellationToken = default)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        log.Timestamp = DateTime.UtcNow;

        await _context.AuthenticationLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuthenticationLog>> GetByUserIdAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.AuthenticationLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuthenticationLog>> GetByIpAddressAsync(string ipAddress, DateTime since, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Enumerable.Empty<AuthenticationLog>();

        return await _context.AuthenticationLogs
            .Where(l => l.IpAddress == ipAddress && l.Timestamp >= since)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetFailedLoginCountAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.AuthenticationLogs
            .CountAsync(l => l.UserId == userId 
                          && l.EventType == AuthenticationEventType.FailedLogin 
                          && l.Timestamp >= since, 
                        cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetFailedLoginCountByIpAsync(string ipAddress, DateTime since, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return 0;

        return await _context.AuthenticationLogs
            .CountAsync(l => l.IpAddress == ipAddress 
                          && l.EventType == AuthenticationEventType.FailedLogin 
                          && l.Timestamp >= since, 
                        cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuthenticationLog>> GetByEventTypeAsync(AuthenticationEventType eventType, DateTime since, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.AuthenticationLogs
            .Where(l => l.EventType == eventType && l.Timestamp >= since)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
