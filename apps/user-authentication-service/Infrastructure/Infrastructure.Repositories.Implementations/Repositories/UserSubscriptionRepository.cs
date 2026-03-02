using Domain.Entities.Entities;
using Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Services.Repositories.Abstractions.Interfaces;

namespace Infrastructure.Repositories.Implementations.Repositories;

/// <summary>
/// Реализация репозитория подписок пользователей
/// Обеспечивает доступ к данным подписок через Entity Framework
/// </summary>
public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly DatabaseContext _context;

    public UserSubscriptionRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<UserSubscription?> GetByIdAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSubscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserSubscription>> GetActiveSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId 
                     && s.IsActive 
                     && s.EndDate > now)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserSubscription?> GetByNopCommerceOrderIdAsync(string nopCommerceOrderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nopCommerceOrderId))
            return null;

        return await _context.UserSubscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.NopCommerceOrderId == nopCommerceOrderId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserSubscription> CreateAsync(UserSubscription subscription, CancellationToken cancellationToken = default)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        subscription.CreatedAt = DateTime.UtcNow;

        await _context.UserSubscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return subscription;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserSubscription subscription, CancellationToken cancellationToken = default)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        _context.UserSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserSubscriptions
            .AnyAsync(s => s.UserId == userId 
                        && s.IsActive 
                        && s.EndDate > now, 
                      cancellationToken);
    }
}
