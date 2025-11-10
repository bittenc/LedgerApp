// ==========================================================
// Project: LedgerApp
// File: OutboxStatus.cs
// Description: Status de processamento da Outbox.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

namespace LedgerApp.Domain.Enums;

public enum OutboxStatus
{
    PENDING = 0,
    SENT    = 1,
    FAILED  = 2
}
