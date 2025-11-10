// ==========================================================
// Project: LedgerApp
// File: TransactionDdb.cs
// Description: Modelo mapeado para a tabela fin_Transactions no DynamoDB.
// Author: Matheus Bittencourt
// Date: 05/11/2025
// ==========================================================

using Amazon.DynamoDBv2.DataModel;
using LedgerApp.Domain.Enums;

namespace LedgerApp.Data.Dynamo.Models;

[DynamoDBTable("fin_Transactions")]
public class TransactionDdb
{
    // Chave de partição (Partition Key) — agrupa as transações por conta
    [DynamoDBHashKey]
    public string AccountId { get; set; } = default!;

    // Chave de ordenação (Sort Key) — ordena as transações cronologicamente
    [DynamoDBRangeKey]
    public long Timestamp { get; set; }

    // Índice global secundário para busca por TransactionId
    [DynamoDBGlobalSecondaryIndexHashKey("gsi_TransactionId")]
    public string TransactionId { get; set; } = default!;

    // Tipo da transação (CREDIT | DEBIT)
    public TransactionType Type { get; set; }

    // Valor da transação
    public decimal Amount { get; set; }

    // Descrição opcional
    public string? Description { get; set; }
}
