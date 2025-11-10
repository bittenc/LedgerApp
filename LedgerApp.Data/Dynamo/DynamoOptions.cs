// ==========================================================
// Project: LedgerApp
// File: DynamoOptions.cs
// Description: Opções de configuração para acesso ao DynamoDB local.
// Author: Matheus Bittencourt
// Date: 05/11/2025
// ==========================================================

namespace LedgerApp.Data.Dynamo;


public class DynamoOptions
{
    public string ServiceUrl { get; set; } = "http://localhost:8000";
    public string Region { get; set; } = "us-east-1";
    public string AccountsTable { get; set; } = "fin_Accounts";
    public string TransactionsTable { get; set; } = "fin_Transactions";
    public string TransactionIdGsi { get; set; } = "gsi_TransactionId";
}
