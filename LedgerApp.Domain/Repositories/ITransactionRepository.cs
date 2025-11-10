// ==========================================================
// Project: LedgerApp
// File: ITransactionRepository.cs
// Description: Contrato de acesso a transações (DynamoDB).
// Author: Matheus Bittencourt
// Date: 05/11/2025
// ==========================================================

namespace LedgerApp.Domain.Repositories;

using LedgerApp.Domain.Entities;

public interface ITransactionRepository
{
    /// Bloco: grava uma transação
    Task CreateAsync(Transaction txn, CancellationToken ct = default);

    /// Bloco: consulta por AccountId com faixa de datas
    Task<IReadOnlyList<Transaction>> GetByAccountAsync(
        Guid accountId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default);

    /// Bloco: consulta por TransactionId (via GSI)
    Task<Transaction?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);
}
