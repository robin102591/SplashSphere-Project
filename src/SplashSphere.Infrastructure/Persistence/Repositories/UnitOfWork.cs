using Microsoft.EntityFrameworkCore.Storage;
using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Infrastructure.Persistence.Repositories;

/// <summary>
/// Thin wrapper over <see cref="ApplicationDbContext"/> that exposes the unit-of-work
/// boundary to command handlers without leaking EF Core types into the Domain layer.
/// <para>
/// Explicit database transactions (<see cref="BeginTransactionAsync"/> /
/// <see cref="CommitTransactionAsync"/> / <see cref="RollbackTransactionAsync"/>)
/// should be reserved for operations that span multiple <see cref="SaveChangesAsync"/>
/// calls (e.g. the <c>CreateTransaction</c> command which writes the transaction header
/// and then updates the queue entry status atomically).
/// </para>
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
