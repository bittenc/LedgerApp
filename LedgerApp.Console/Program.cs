// ==========================================================
// Project: LedgerApp
// File: Program.cs
// Description: Console host para DI, configuração e teste rápido dos repositórios.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using LedgerApp.Domain.Repositories;
using LedgerApp.Data.Dynamo;

// Top-level statements
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        // Config Dynamo (local ou AWS)
        var serviceUrl = ctx.Configuration["AWS:ServiceURL"] ?? "http://localhost:8000";
        var region     = ctx.Configuration["AWS:Region"]     ?? "us-east-1";

        services.AddSingleton<IAmazonDynamoDB>(_ =>
        {
            if (!string.IsNullOrWhiteSpace(serviceUrl))
            {
                return new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = serviceUrl,
                    UseHttp    = true
                });
            }

            return new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(region));
        });

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

        // Repositórios
        services.AddScoped<IAccountRepository, AccountRepositoryDynamo>();
        services.AddScoped<ITransactionRepository, TransactionRepositoryDynamo>();
        services.AddScoped<IOutboxRepository, OutboxRepositoryDynamo>();
        services.AddScoped<LedgerApp.Data.Services.LedgerService>();

        // HostedService que publica mensagens do Outbox no RabbitMQ
        services.AddHostedService<LedgerApp.Console.OutboxDispatcher>();
    });

using var host = builder.Build();

// Smoke test: Seed a transaction if SEED_TXN=1
if (Environment.GetEnvironmentVariable("SEED_TXN") == "1")
{
    using var scope = host.Services.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<LedgerApp.Data.Services.LedgerService>();

    var accId = Guid.Parse("11111111-2222-3333-4444-555555555555"); // use a conta que você já inseriu
    await svc.CreateTransactionAsync(accId, LedgerApp.Domain.Enums.TransactionType.CREDIT, 10.00m, "Seed txn");
}


// Validação simples de DI
var accountRepo = host.Services.GetRequiredService<IAccountRepository>();
System.Console.WriteLine("Console up ✅ — DI, Dynamo e OutboxDispatcher registrados.");

await host.RunAsync();




