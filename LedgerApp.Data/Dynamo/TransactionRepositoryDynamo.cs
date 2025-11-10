// ==========================================================
// Project: LedgerApp
// File: TransactionRepositoryDynamo.cs
// Description: Implementação do repositório de transações no DynamoDB.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using LedgerApp.Data.Dynamo.Models;
using LedgerApp.Domain.Entities;
using LedgerApp.Domain.Repositories;

namespace LedgerApp.Data.Dynamo;

/// Repositório de transações no DynamoDB
public class TransactionRepositoryDynamo : ITransactionRepository
{
    private readonly IDynamoDBContext _ctx;

    public TransactionRepositoryDynamo(IDynamoDBContext ctx)
    {
        _ctx = ctx;
    }

    /// Cria a transação
    public async Task CreateAsync(Transaction transaction, CancellationToken ct = default)
    {
        var model = new TransactionDdb
        {
            AccountId     = transaction.AccountId.ToString(),
            Timestamp     = transaction.OccurredAt.ToUnixTimeSeconds(),
            TransactionId = transaction.Id.ToString(),
            Type          = transaction.Type,          
            Amount        = transaction.Amount,
            Description   = transaction.Description
        };

        await _ctx.SaveAsync(model, new SaveConfig(), ct);
    }

    /// Busca por TransactionId via GSI
    public async Task<Transaction?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        var q = _ctx.QueryAsync<TransactionDdb>(
            transactionId.ToString(),
            new QueryConfig { IndexName = "gsi_TransactionId" }
        );

        var items = await q.GetRemainingAsync(ct);
        var item  = items.FirstOrDefault();
        if (item is null) return null;

        return new Transaction
        {
            Id          = Guid.Parse(item.TransactionId),
            AccountId   = Guid.Parse(item.AccountId),
            Type        = item.Type,
            Amount      = item.Amount,
            Description = item.Description ?? string.Empty,
            OccurredAt  = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp)
        };
    }

    /// Lista por conta com filtros de período opcionais 
    public async Task<IReadOnlyList<Transaction>> GetByAccountAsync(
        Guid accountId,
        DateTimeOffset? from = null,
        DateTimeOffset? to   = null,
        CancellationToken ct = default)
    {
        var q = _ctx.QueryAsync<TransactionDdb>(
            accountId.ToString(),
            new QueryConfig() 
        );

        var models = await q.GetRemainingAsync(ct);

        // Filtro em memória (ok para ambiente local/baixo volume)
        if (from.HasValue)
        {
            var f = from.Value.ToUnixTimeSeconds();
            models = models.Where(m => m.Timestamp >= f).ToList();
        }
        if (to.HasValue)
        {
            var t = to.Value.ToUnixTimeSeconds();
            models = models.Where(m => m.Timestamp <= t).ToList();
        }

        return models
            .Select(m => new Transaction
            {
                Id          = Guid.Parse(m.TransactionId),
                AccountId   = Guid.Parse(m.AccountId),
                Type        = m.Type,
                Amount      = m.Amount,
                Description = m.Description ?? string.Empty,
                OccurredAt  = DateTimeOffset.FromUnixTimeSeconds(m.Timestamp)
            })
            .ToList();
    }
}
