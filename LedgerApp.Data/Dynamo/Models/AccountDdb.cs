// ==========================================================
// Project: LedgerApp
// File: AccountDdb.cs
// Description: Modelo mapeado para a tabela DynamoDB fin_Accounts.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using Amazon.DynamoDBv2.DataModel;

namespace LedgerApp.Data.Dynamo.Models;

/// Modelo persistido no DynamoDB (tabela fin_Accounts)
[DynamoDBTable("fin_Accounts")]
public class AccountDdb
{
    /// PK da tabela (string com o Guid)
    [DynamoDBHashKey]
    public string Id { get; set; } = default!;

    /// Nome do titular
    public string OwnerName { get; set; } = string.Empty;

    /// Número da conta
    public string Number { get; set; } = string.Empty;

    /// Saldo
    public decimal Balance { get; set; }
}
