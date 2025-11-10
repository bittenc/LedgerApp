// ==========================================================
// Project: LedgerApp
// File: IOutboxRepository.cs
// Description: Contrato de persistência para Outbox.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using LedgerApp.Domain.Entities;

namespace LedgerApp.Domain.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int limit = 50,
        CancellationToken ct = default);

    Task MarkAsSentAsync(Guid id, CancellationToken ct = default);

    Task MarkAsFailedAsync(Guid id, string error, CancellationToken ct = default);
}