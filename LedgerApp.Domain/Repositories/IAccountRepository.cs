// ==========================================================
// Project: LedgerApp
// File: IAccountRepository.cs
// Description: Contrato de acesso a contas (DynamoDB).
// Author: Matheus Bittencourt
// Date: 05/11/2025
// ==========================================================

namespace LedgerApp.Domain.Repositories;

using LedgerApp.Domain.Entities;

public interface IAccountRepository
{
    /// Bloco: cria ou atualiza uma conta
    Task UpsertAsync(Account account, CancellationToken ct = default);

    /// Bloco: obtém uma conta pelo Id
    Task<Account?> GetAsync(Guid id, CancellationToken ct = default);
}
