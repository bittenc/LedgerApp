// ==========================================================
// Project: LedgerApp
// File: Account.cs
// Description: Entidade de conta (domínio) com número, titular e saldo.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

namespace LedgerApp.Domain.Entities;

/// Modelo de domínio da conta
public class Account
{
    /// Identificador único da conta
    public Guid Id { get; set; }

    /// Nome do titular/cliente da conta
    public string OwnerName { get; set; } = string.Empty;

    /// Número da conta (ex.: "0001-9")
    public string Number { get; set; } = string.Empty;

    /// Saldo atual
    public decimal Balance { get; set; }
}
