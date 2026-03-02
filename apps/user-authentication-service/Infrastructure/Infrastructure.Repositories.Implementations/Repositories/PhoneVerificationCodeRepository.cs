using Domain.Entities.Entities;
using Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Services.Repositories.Abstractions.Interfaces;

namespace Infrastructure.Repositories.Implementations.Repositories;

/// <summary>
/// Реализация репозитория кодов подтверждения телефона
/// Обеспечивает доступ к данным кодов верификации через Entity Framework
/// </summary>
public class PhoneVerificationCodeRepository : IPhoneVerificationCodeRepository
{
    private readonly DatabaseContext _context;

    public PhoneVerificationCodeRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<PhoneVerificationCode?> GetActiveCodeAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var now = DateTime.UtcNow;
        return await _context.PhoneVerificationCodes
            .Where(c => c.PhoneNumber == phoneNumber 
                     && !c.IsUsed 
                     && c.ExpiresAt > now)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PhoneVerificationCode> CreateAsync(PhoneVerificationCode code, CancellationToken cancellationToken = default)
    {
        if (code == null)
            throw new ArgumentNullException(nameof(code));

        code.CreatedAt = DateTime.UtcNow;
        code.IsUsed = false;
        code.AttemptCount = 0;

        await _context.PhoneVerificationCodes.AddAsync(code, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return code;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(PhoneVerificationCode code, CancellationToken cancellationToken = default)
    {
        if (code == null)
            throw new ArgumentNullException(nameof(code));

        _context.PhoneVerificationCodes.Update(code);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAsUsedAsync(int codeId, CancellationToken cancellationToken = default)
    {
        var code = await _context.PhoneVerificationCodes
            .FirstOrDefaultAsync(c => c.Id == codeId, cancellationToken);

        if (code != null)
        {
            code.IsUsed = true;
            code.UsedAt = DateTime.UtcNow;
            
            _context.PhoneVerificationCodes.Update(code);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetSentCountSinceAsync(string phoneNumber, DateTime since, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return 0;

        return await _context.PhoneVerificationCodes
            .CountAsync(c => c.PhoneNumber == phoneNumber 
                          && c.CreatedAt >= since, 
                        cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteExpiredCodesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredCodes = await _context.PhoneVerificationCodes
            .Where(c => c.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expiredCodes.Any())
        {
            _context.PhoneVerificationCodes.RemoveRange(expiredCodes);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
