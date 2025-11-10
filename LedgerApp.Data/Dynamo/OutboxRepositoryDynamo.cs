// ==========================================================
// Project: LedgerApp
// File: OutboxRepositoryDynamo.cs
// Description: Implementação DynamoDB do repositório de outbox.
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel; // DynamoDBContext + ScanCondition
using LedgerApp.Data.Dynamo.Models;
using LedgerApp.Domain.Entities;
using LedgerApp.Domain.Repositories;

namespace LedgerApp.Data.Dynamo
{
    public class OutboxRepositoryDynamo : IOutboxRepository
    {
        private readonly IDynamoDBContext _ctx;

        public OutboxRepositoryDynamo(IDynamoDBContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
        {
            var model = new OutboxMessageDdb
            {
                Id            = message.Id.ToString(),
                AggregateId   = message.AggregateId.ToString(),
                EventType     = message.EventType,
                PayloadJson   = message.PayloadJson,
                Status        = message.Status,
                Error         = message.Error,
                CreatedAt     = message.CreatedAt.ToUnixTimeSeconds(),
                SentAt        = message.SentAt?.ToUnixTimeSeconds(),
                LastAttemptAt = message.LastAttemptAt?.ToUnixTimeSeconds(),
                Attempts      = message.Attempts
            };

            await _ctx.SaveAsync(model, new SaveConfig(), ct);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(int limit = 50, CancellationToken ct = default)
        {
            // Scan sem condições e filtro em memória (suficiente para lab/local).
            var scan = _ctx.ScanAsync<OutboxMessageDdb>(new List<ScanCondition>()); 
            var all  = await scan.GetRemainingAsync(ct);



            var pendings = all
                .Where(m => string.Equals(m.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.CreatedAt)
                .Take(limit)
                .Select(m => new OutboxMessage
                {
                    Id            = Guid.Parse(m.Id),
                    AggregateId   = Guid.Parse(m.AggregateId),
                    EventType     = m.EventType,
                    PayloadJson   = m.PayloadJson,
                    Status        = m.Status,
                    Error         = m.Error,
                    CreatedAt     = DateTimeOffset.FromUnixTimeSeconds(m.CreatedAt),
                    SentAt        = m.SentAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(m.SentAt.Value) : null,
                    LastAttemptAt = m.LastAttemptAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(m.LastAttemptAt.Value) : null,
                    Attempts      = m.Attempts
                })
                .ToList();

            return pendings;
        }

        public async Task MarkAsSentAsync(Guid id, CancellationToken ct = default)
        {
            var model = await _ctx.LoadAsync<OutboxMessageDdb>(id.ToString(), new LoadConfig(), ct);
            if (model is null) return;

            model.Status = "SENT";
            model.SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            model.Error  = null;

            await _ctx.SaveAsync(model, new SaveConfig(), ct);
        }

        public async Task MarkAsFailedAsync(Guid id, string error, CancellationToken ct = default)
        {
            var model = await _ctx.LoadAsync<OutboxMessageDdb>(id.ToString(), new LoadConfig(), ct);
            if (model is null) return;

            model.Status        = "FAILED";
            model.Error         = error;
            model.LastAttemptAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            model.Attempts     += 1;

            await _ctx.SaveAsync(model, new SaveConfig(), ct);
        }
    }
}
