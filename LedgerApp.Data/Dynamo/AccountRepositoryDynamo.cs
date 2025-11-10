// ==========================================================
// Project: LedgerApp
// File: AccountRepositoryDynamo.cs
// Description: Implementação do repositório de contas no DynamoDB.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using Amazon.DynamoDBv2.DataModel;
using LedgerApp.Data.Dynamo.Models;
using LedgerApp.Domain.Entities;
using LedgerApp.Domain.Repositories;

namespace LedgerApp.Data.Dynamo;

/// Repositório de contas no DynamoDB
public class AccountRepositoryDynamo : IAccountRepository
{
    private readonly IDynamoDBContext _ctx;

    public AccountRepositoryDynamo(IDynamoDBContext ctx)
    {
        _ctx = ctx;
    }

    /// Upsert da conta
    public async Task UpsertAsync(Account account, CancellationToken ct = default)
    {
        var model = new AccountDdb
        {
            Id        = account.Id.ToString(),
            OwnerName = account.OwnerName,
            Number    = account.Number,
            Balance   = account.Balance
        };

        await _ctx.SaveAsync(model, new SaveConfig(), ct);
    }

    /// Busca conta por Id
    public async Task<Account?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var model = await _ctx.LoadAsync<AccountDdb>(id.ToString(), new LoadConfig(), ct);
        if (model is null) return null;

        return new Account
        {
            Id        = Guid.Parse(model.Id),
            OwnerName = model.OwnerName,
            Number    = model.Number,
            Balance   = model.Balance
        };
    }
}
