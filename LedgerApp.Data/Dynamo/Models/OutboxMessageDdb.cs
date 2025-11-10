// ==========================================================
// Project: LedgerApp
// File: OutboxMessageDdb.cs
// Description: Modelo mapeado para a tabela fin_Outbox no DynamoDB.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using Amazon.DynamoDBv2.DataModel;

namespace LedgerApp.Data.Dynamo.Models;

[DynamoDBTable("fin_Outbox")]
public class OutboxMessageDdb
{
    [DynamoDBHashKey]               // PK: Id
    public string Id { get; set; } = default!;

    public string AggregateId { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public string PayloadJson { get; set; } = default!;
    public string Status { get; set; } = "PENDING";
    public string? Error { get; set; }

    public long CreatedAt { get; set; }           // epoch seconds
    public long? SentAt { get; set; }
    public long? LastAttemptAt { get; set; }
    public int Attempts { get; set; }
}