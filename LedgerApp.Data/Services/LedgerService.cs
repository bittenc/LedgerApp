// ==========================================================
// Project: LedgerApp
// File: LedgerService.cs
// Description: Serviço de domínio para registrar transações e enfileirar eventos na outbox.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================
using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using LedgerApp.Data.Dynamo.Models;
using LedgerApp.Domain.Entities;
using LedgerApp.Domain.Enums;
using LedgerApp.Domain.Repositories;

namespace LedgerApp.Data.Services;

public class LedgerService
{
    private readonly IDynamoDBContext _ctx;
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _txns;
    private readonly IOutboxRepository _outbox;

    public LedgerService(
        IDynamoDBContext ctx,
        IAccountRepository accounts,
        ITransactionRepository txns,
        IOutboxRepository outbox)
    {
        _ctx = ctx;
        _accounts = accounts;
        _txns = txns;
        _outbox = outbox;
    }

    public async Task<Transaction> CreateTransactionAsync(
        Guid accountId, TransactionType type, decimal amount, string? description, CancellationToken ct = default)
    {
        var account = await _accounts.GetAsync(accountId, ct) ??
                      throw new InvalidOperationException("Conta não encontrada.");

        var txn = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            Amount = amount,
            Description = description ?? string.Empty,
            OccurredAt = DateTimeOffset.UtcNow
        };

        if (type == TransactionType.DEBIT)
            account.Balance -= amount;
        else
            account.Balance += amount;

        var txnDdb = new TransactionDdb
        {
            AccountId     = account.Id.ToString(),
            Timestamp     = txn.OccurredAt.ToUnixTimeSeconds(),
            TransactionId = txn.Id.ToString(),
            Type          = txn.Type,
            Amount        = txn.Amount,
            Description   = txn.Description
        };

        var accountDdb = new AccountDdb
        {
            Id        = account.Id.ToString(),
            OwnerName = account.OwnerName,
            Number    = account.Number,
            Balance   = account.Balance
        };

        var outbox = new OutboxMessage
        {
            Id          = Guid.NewGuid(),
            AggregateId = txn.Id,
            EventType   = "ledger.transaction.created",
            PayloadJson = JsonSerializer.Serialize(new
            {
                transactionId = txn.Id,
                accountId = txn.AccountId,
                type = txn.Type.ToString(),
                amount = txn.Amount,
                description = txn.Description,
                occurredAt = txn.OccurredAt
            }),
            Status      = "PENDING",
            CreatedAt   = DateTimeOffset.UtcNow
        };

        var batch = _ctx.CreateBatchWrite<OutboxMessageDdb>();
        // Como BatchWrite do DataModel não mistura tipos diferentes,
        // fazemos dois saves e um AddAsync(*) do Outbox fora do batch.
        await _ctx.SaveAsync(accountDdb, new SaveConfig(), ct);
        await _ctx.SaveAsync(txnDdb, new SaveConfig(), ct);
        await _outbox.AddAsync(outbox, ct);

        return txn;
    }
}    