// ==========================================================
// Project: LedgerApp
// File: OutboxMessage.cs
// Description: Entidade de domínio para padrão Outbox.
// ==========================================================

namespace LedgerApp.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }                  // PK do outbox
    public Guid AggregateId { get; set; }         // Ex.: TransactionId
    public string EventType { get; set; } = "";   // Ex.: "ledger.transaction.created"
    public string PayloadJson { get; set; } = ""; // JSON do evento

    public string Status { get; set; } = "PENDING";   // PENDING | SENT | FAILED
    public string? Error { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public int Attempts { get; set; }
}
