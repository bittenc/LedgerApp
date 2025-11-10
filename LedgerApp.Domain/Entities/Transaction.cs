// ==========================================================
// Project: LedgerApp
// File: Transaction.cs
// Description: Entidade de domínio que representa uma transação financeira.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using LedgerApp.Domain.Enums;

namespace LedgerApp.Domain.Entities;


public class Transaction
{
    // Identificador único da transação
    public Guid Id { get; set; }

    // Identificador da conta vinculada
    public Guid AccountId { get; set; }

    // Tipo da transação (CREDIT | DEBIT)
    public TransactionType Type { get; set; }

    // Valor da transação
    public decimal Amount { get; set; }

    // Descrição opcional
    public string? Description { get; set; }

    // Data/hora em que a transação ocorreu
    public DateTimeOffset OccurredAt { get; set; }
}
