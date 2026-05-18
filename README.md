# CashFlow - Controle de Fluxo de Caixa

Sistema de controle de fluxo de caixa diário para comerciantes, composto por dois microsserviços em .NET 10. Permite registrar lançamentos financeiros (débitos e créditos) e consultar o saldo consolidado por dia, com autenticação via Keycloak.

> Detalhes de arquitetura, decisões técnicas, modelo de domínio e design de APIs estão documentados em [docs/SDD.md](docs/SDD.md).

---

## Serviços

| Serviço | Porta |
|---------|-------|
| Transactions API | 8080 |
| Consolidation API | 8081 |
| Keycloak (Identity Provider) | 9080 |
| RabbitMQ Management UI | 15672 |

---

## Como Rodar

### Pré-requisitos
- Docker 24+
- Docker Compose v2

### Subir todos os serviços

```bash
docker compose up -d
```

O **Keycloak demora ~90 segundos** para inicializar e importar o realm — as APIs só sobem após ele estar pronto.

```bash
docker compose ps   # aguarde todos os serviços com status "healthy"
```

### Verificar saúde

```bash
curl http://localhost:8080/health
curl http://localhost:8081/health
curl http://localhost:9080/health/ready
```

### Parar

```bash
docker compose down        # mantém volumes
docker compose down -v     # remove volumes também
```

---

## Como Rodar Sem Docker

### Pré-requisitos
- .NET 10 SDK
- PostgreSQL 16, RabbitMQ 3.13, Redis 7, Keycloak 26

### 1. Subir dependências via Docker

```bash
docker run -d --name pg-tx -e POSTGRES_USER=cashflow -e POSTGRES_PASSWORD=cashflow123 -e POSTGRES_DB=cashflow_transactions -p 5432:5432 postgres:16-alpine
docker run -d --name pg-cons -e POSTGRES_USER=cashflow -e POSTGRES_PASSWORD=cashflow123 -e POSTGRES_DB=cashflow_consolidation -p 5433:5432 postgres:16-alpine
docker run -d --name rabbit -e RABBITMQ_DEFAULT_USER=cashflow -e RABBITMQ_DEFAULT_PASS=cashflow123 -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management-alpine
docker run -d --name redis -p 6379:6379 redis:7-alpine
docker run -d --name keycloak \
  -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin -e KC_HEALTH_ENABLED=true \
  -v "$(pwd)/keycloak:/opt/keycloak/data/import:ro" \
  -p 9080:8080 quay.io/keycloak/keycloak:26.0 start-dev --import-realm
```

### 2. Rodar os serviços

```bash
# Terminal 1
cd src/CashFlow.Transactions && dotnet run

# Terminal 2
cd src/CashFlow.Consolidation && dotnet run
```

As migrations são aplicadas automaticamente na inicialização.

---

## Autenticação

Todos os endpoints exigem um token JWT obtido do Keycloak.

### Obter token

```bash
TOKEN=$(curl -s -X POST http://localhost:9080/realms/cashflow/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=cashflow-api&client_secret=cashflow-api-secret&username=cashflow-admin&password=admin123" \
  | jq -r '.access_token')
```

### Usuários disponíveis

| Usuário | Senha | Acesso |
|---------|-------|--------|
| `cashflow-admin` | `admin123` | Leitura e escrita |
| `cashflow-reader` | `reader123` | Apenas leitura |

Keycloak Admin UI: http://localhost:9080/admin (admin / admin)

---

## Endpoints

O Swagger de cada serviço também está disponível em `/swagger`.

### Transactions API — http://localhost:8080

```bash
# Criar lançamento
curl -X POST http://localhost:8080/api/v1/transactions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"amount": 1500.00, "type": "credit", "description": "Venda de produto", "occurredAt": "2024-01-15T10:30:00Z"}'

# Listar lançamentos
curl http://localhost:8080/api/v1/transactions -H "Authorization: Bearer $TOKEN"

# Listar por data
curl "http://localhost:8080/api/v1/transactions?date=2024-01-15" -H "Authorization: Bearer $TOKEN"
```

### Consolidation API — http://localhost:8081

```bash
# Consultar saldo consolidado por data
curl http://localhost:8081/api/v1/consolidation/2024-01-15 -H "Authorization: Bearer $TOKEN"
```

---

## Testes

```bash
dotnet test
```

11 testes passando (5 em Transactions, 6 em Consolidation).
