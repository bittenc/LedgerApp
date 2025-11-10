**Arquitetura e Objetivos**

A LedgerApp é uma aplicação modular dividida em três camadas:

**- LedgerApp.Domain**

Entidades de negócio: Account, Transaction, OutboxMessage

Enums: TransactionType, OutboxStatus

Contratos: IAccountRepository, ITransactionRepository, IOutboxRepository


**- LedgerApp.Data**

Persistência em Amazon DynamoDB usando IDynamoDBContext

Modelos mapeados para tabelas: AccountDdb, TransactionDdb, OutboxMessageDdb

Repositórios: AccountRepositoryDynamo, TransactionRepositoryDynamo, OutboxRepositoryDynamo

Serviço de domínio: LedgerService (responsável por registrar transações e escrever na outbox)


**- LedgerApp.Console**

Host .NET 8 com Host.CreateDefaultBuilder

Configuração de DI para repositórios e DynamoDB

OutboxDispatcher (HostedService) que lê mensagens pendentes da outbox e publica no RabbitMQ

**Padrão Outbox**

Usado para garantir consistência entre escrita no banco e publicação de eventos:

A transação é gravada nas tabelas fin_Accounts e fin_Transactions.

Um registro é inserido na tabela fin_Outbox com status PENDING.

Um processo em background (OutboxDispatcher) consulta a outbox periodicamente:

Publica mensagens pendentes no RabbitMQ

Atualiza o status para SENT ou FAILED em caso de erro

-----------------------------------------------------------------------------------------------

**Stack Tecnológica**

.NET 8 (C#)

Amazon DynamoDB Local (via Docker)

RabbitMQ 3 Management (via Docker)

AWS CLI para criar e inspecionar tabelas

RabbitMQ.Client para publicação de mensagens

Microsoft.Extensions.Hosting para DI + HostedService

System.Text.Json para serialização de payloads

-----------------------------------------------------------------------------------------------

**Setup do Ambiente Local**

1. Dependências

.NET 8 SDK

Docker

AWS CLI

2. Subir DynamoDB Local e RabbitMQ
docker rm -f dynamodb-local rabbit 2>/dev/null || true

# DynamoDB Local
docker run -d --name dynamodb-local -p 8000:8000 amazon/dynamodb-local

# RabbitMQ (com painel de administração)
docker run -d --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management


RabbitMQ Management UI: http://localhost:15672
 (usuário: guest, senha: guest)
 
-----------------------------------------------------------------------------------------------

**Criação das Tabelas DynamoDB**

export AWS_PAGER=""
export AWS_DEFAULT_REGION=us-east-1

# fin_Accounts
aws dynamodb create-table \
  --endpoint-url http://localhost:8000 \
  --table-name fin_Accounts \
  --attribute-definitions AttributeName=Id,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST

# fin_Transactions
aws dynamodb create-table \
  --endpoint-url http://localhost:8000 \
  --table-name fin_Transactions \
  --attribute-definitions \
      AttributeName=AccountId,AttributeType=S \
      AttributeName=Timestamp,AttributeType=N \
      AttributeName=TransactionId,AttributeType=S \
  --key-schema \
      AttributeName=AccountId,KeyType=HASH \
      AttributeName=Timestamp,KeyType=RANGE \
  --global-secondary-indexes '[
    {
      "IndexName": "gsi_TransactionId",
      "KeySchema": [{"AttributeName": "TransactionId","KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"}
    }
  ]' \
  --billing-mode PAY_PER_REQUEST

# fin_Outbox
aws dynamodb create-table \
  --endpoint-url http://localhost:8000 \
  --table-name fin_Outbox \
  --attribute-definitions AttributeName=Id,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST


Verificar:

aws dynamodb list-tables --endpoint-url http://localhost:8000

-----------------------------------------------------------------------------------------------

**Execução da Aplicação**

1. Variáveis de ambiente
export AWS__Region=us-east-1
export AWS__ServiceURL=http://localhost:8000

export RABBIT__Host=localhost
export RABBIT__Port=5672
export RABBIT__User=guest
export RABBIT__Pass=guest
export RABBIT__Exchange=ledger.events
export RABBIT__RoutingKey=ledger.transaction.created

2. Rodando com transação de seed
export SEED_TXN=1

dotnet build
dotnet run --project LedgerApp.Console
