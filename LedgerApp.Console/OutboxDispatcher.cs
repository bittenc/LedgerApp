// ==========================================================
// Project: LedgerApp
// File: OutboxDispatcher.cs
// Description: HostedService que publica mensagens do Outbox no RabbitMQ.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

using LedgerApp.Domain.Repositories;

namespace LedgerApp.Console;

public class OutboxDispatcher : BackgroundService
{
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IConfiguration _cfg;
    private readonly IOutboxRepository _outbox;

    public OutboxDispatcher(
        ILogger<OutboxDispatcher> logger,
        IConfiguration cfg,
        IOutboxRepository outbox)
    {
        _logger = logger;
        _cfg = cfg;
        _outbox = outbox;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host      = _cfg["RABBIT:Host"]        ?? "localhost";
        var portStr   = _cfg["RABBIT:Port"]        ?? "5672";
        var user      = _cfg["RABBIT:User"]        ?? "guest";
        var pass      = _cfg["RABBIT:Pass"]        ?? "guest";
        var exchange  = _cfg["RABBIT:Exchange"]    ?? "ledger.events";
        var routingKey= _cfg["RABBIT:RoutingKey"]  ?? "ledger.transaction.created";

        var port = int.TryParse(portStr, out var p) ? p : 5672;

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass
        };

        using var conn = factory.CreateConnection();
        using var ch   = conn.CreateModel();

        ch.ExchangeDeclare(exchange, type: "topic", durable: true, autoDelete: false);

        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = await _outbox.GetPendingBatchAsync(limit: 25, stoppingToken);
            if (batch.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            foreach (var msg in batch)
            {
                try
                {
                    var body = Encoding.UTF8.GetBytes(msg.PayloadJson);
                    ch.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);

                    await _outbox.MarkAsSentAsync(msg.Id, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao publicar mensagem Outbox {OutboxId}", msg.Id);
                    await _outbox.MarkAsFailedAsync(msg.Id, ex.Message, stoppingToken);
                }
            }
        }
    }
}