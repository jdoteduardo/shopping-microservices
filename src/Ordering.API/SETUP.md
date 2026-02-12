# INSTRUÇÕES DE SETUP - Ordering.API

## ?? Pré-requisitos

1. .NET 8 SDK instalado
2. MongoDB (Docker ou instalação local)
3. Editor de código (Visual Studio, VS Code, ou Rider)

## ?? Passo a Passo para Executar

### 1. Navegar até o projeto

```powershell
cd c:\CursoWebApi\projetos\shop-microservices\src\Ordering.API
```

### 2. Restaurar dependências

```powershell
dotnet restore
```

### 3. Configurar MongoDB

**Opção A - MongoDB via Docker (Recomendado):**

```powershell
docker run -d --name mongodb -p 27017:27017 mongo:latest
```

**Opção B - MongoDB com persistência:**

```powershell
docker run -d --name mongodb -p 27017:27017 -v mongodb-data:/data/db mongo:latest
```

**Opção C - MongoDB com autenticação:**

```powershell
docker run -d --name mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=password123 mongo:latest
```

Se usar autenticação, altere em `appsettings.json`:

```json
"MongoDbSettings": {
  "ConnectionString": "mongodb://admin:password123@localhost:27017",
  "DatabaseName": "OrderingDb",
  "OrdersCollectionName": "orders"
}
```

### 4. Executar a aplicação

```powershell
dotnet run
```

### 5. Testar a API

**Swagger UI:**
Abra o navegador em: `http://localhost:5003`

**Health Check:**

```powershell
curl http://localhost:5003/health
```

**Testar endpoints via PowerShell:**

```powershell
# GET - Listar todos os pedidos
Invoke-RestMethod -Uri "http://localhost:5003/api/orders" -Method Get

# POST - Criar novo pedido
$orderBody = @{
    userId = "user123"
    items = @(
        @{
            productId = 1
            productName = "Laptop"
            price = 999.99
            quantity = 1
        },
        @{
            productId = 2
            productName = "Mouse"
            price = 29.99
            quantity = 2
        }
    )
    shippingAddress = @{
        street = "123 Main Street"
        city = "São Paulo"
        state = "SP"
        zipCode = "01310-100"
        country = "Brazil"
    }
} | ConvertTo-Json -Depth 3

$newOrder = Invoke-RestMethod -Uri "http://localhost:5003/api/orders" -Method Post -Body $orderBody -ContentType "application/json"
$newOrder

# GET - Obter pedido por ID (use o ID retornado acima)
Invoke-RestMethod -Uri "http://localhost:5003/api/orders/$($newOrder.id)" -Method Get

# GET - Listar pedidos de um usuário
Invoke-RestMethod -Uri "http://localhost:5003/api/orders/user/user123" -Method Get

# PUT - Confirmar pedido (atualizar status para Confirmed)
$statusBody = @{
    status = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5003/api/orders/$($newOrder.id)/status" -Method Put -Body $statusBody -ContentType "application/json"

# PUT - Marcar como enviado (status = Shipped)
$statusBody = @{
    status = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5003/api/orders/$($newOrder.id)/status" -Method Put -Body $statusBody -ContentType "application/json"

# PUT - Marcar como entregue (status = Delivered)
$statusBody = @{
    status = 3
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5003/api/orders/$($newOrder.id)/status" -Method Put -Body $statusBody -ContentType "application/json"

# POST - Criar outro pedido para testar cancelamento
$orderBody2 = @{
    userId = "user456"
    items = @(
        @{
            productId = 3
            productName = "Keyboard"
            price = 79.99
            quantity = 1
        }
    )
    shippingAddress = @{
        street = "456 Oak Avenue"
        city = "Rio de Janeiro"
        state = "RJ"
        zipCode = "20040-020"
        country = "Brazil"
    }
} | ConvertTo-Json -Depth 3

$orderToCancel = Invoke-RestMethod -Uri "http://localhost:5003/api/orders" -Method Post -Body $orderBody2 -ContentType "application/json"

# DELETE - Cancelar pedido (apenas pedidos Pending ou Confirmed)
Invoke-RestMethod -Uri "http://localhost:5003/api/orders/$($orderToCancel.id)" -Method Delete
```

## ?? Comandos Úteis

### Verificar logs

```powershell
# Ver logs em tempo real
Get-Content .\Logs\ordering-api-*.log -Wait
```

### Limpar e recompilar

```powershell
dotnet clean
dotnet build
```

### Executar em modo watch (auto-reload)

```powershell
dotnet watch run
```

### Comandos MongoDB úteis

```powershell
# Conectar ao MongoDB Shell via Docker
docker exec -it mongodb mongosh

# Comandos dentro do mongosh:
# Selecionar banco de dados
use OrderingDb

# Ver todas as coleções
show collections

# Listar todos os pedidos
db.orders.find().pretty()

# Buscar pedido por OrderNumber
db.orders.findOne({ orderNumber: "ORD-20240101120000-1234" })

# Buscar pedidos de um usuário
db.orders.find({ userId: "user123" }).pretty()

# Contar pedidos por status
db.orders.aggregate([
  { $group: { _id: "$status", count: { $sum: 1 } } }
])

# Ver índices
db.orders.getIndexes()

# Limpar coleção (CUIDADO!)
db.orders.deleteMany({})
```

## ?? Estrutura de Dados

### Order (Pedido)

```json
{
  "id": "507f1f77bcf86cd799439011",
  "orderNumber": "ORD-20240115143022-5678",
  "userId": "user123",
  "orderDate": "2024-01-15T14:30:22Z",
  "status": "Pending",
  "statusDescription": "Pending",
  "totalAmount": 1059.97,
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "price": 999.99,
      "quantity": 1,
      "subtotal": 999.99
    },
    {
      "productId": 2,
      "productName": "Mouse",
      "price": 29.99,
      "quantity": 2,
      "subtotal": 59.98
    }
  ],
  "shippingAddress": {
    "street": "123 Main Street",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01310-100",
    "country": "Brazil"
  }
}
```

### Order Status (Enum)

| Valor | Nome | Descrição |
|-------|------|-----------|
| 0 | Pending | Pedido criado, aguardando confirmação |
| 1 | Confirmed | Pedido confirmado |
| 2 | Shipped | Pedido enviado |
| 3 | Delivered | Pedido entregue |
| 4 | Cancelled | Pedido cancelado |

### Transições de Status Válidas

```
Pending ? Confirmed
Pending ? Cancelled
Confirmed ? Shipped
Confirmed ? Cancelled
Shipped ? Delivered
```

### Características do MongoDB

- **Database**: OrderingDb
- **Collection**: orders
- **Índices**:
  - `orderNumber` (único)
  - `userId`
  - `orderDate` (descendente)
  - `userId + orderDate` (composto)

## ?? Troubleshooting

### Erro: "Cannot connect to MongoDB"

- Verifique se o MongoDB está rodando:
  ```powershell
  docker ps | Select-String mongodb
  ```
- Teste a conexão:
  ```powershell
  docker exec -it mongodb mongosh --eval "db.runCommand({ ping: 1 })"
  # Deve retornar: { ok: 1 }
  ```
- Confirme as configurações em `appsettings.json`

### Erro: "MongoDB connection timeout"

- Verifique se a porta 27017 está acessível
- Verifique firewall e configurações de rede

### Erro: "Invalid status transition"

- Verifique o status atual do pedido
- Consulte a tabela de transições válidas acima
- Pedidos só podem ser cancelados se estiverem Pending ou Confirmed

### Porta 5003 já em uso

Altere a porta em `Properties/launchSettings.json`

### Container MongoDB não inicia

```powershell
# Remover container antigo
docker rm -f mongodb

# Criar novo
docker run -d --name mongodb -p 27017:27017 mongo:latest
```

### Limpar cache do NuGet

```powershell
dotnet nuget locals all --clear
```

## ?? Variáveis de Ambiente

Para ambiente de produção, configure:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:MongoDbSettings__ConnectionString="mongodb://user:password@mongodb-server:27017"
$env:MongoDbSettings__DatabaseName="OrderingDb"
```

## ?? Configuração MongoDB para Produção

Para produção, use uma connection string mais robusta:

```json
"MongoDbSettings": {
  "ConnectionString": "mongodb://user:password@mongodb-server:27017/?authSource=admin&retryWrites=true&w=majority",
  "DatabaseName": "OrderingDb",
  "OrdersCollectionName": "orders"
}
```

### Opções adicionais de connection string:

- `authSource=admin` - Database de autenticação
- `retryWrites=true` - Retry automático de escritas
- `w=majority` - Write concern para replicação
- `maxPoolSize=100` - Tamanho máximo do pool de conexões
- `connectTimeoutMS=10000` - Timeout de conexão

## ? Checklist de Validação

- [ ] MongoDB está rodando
- [ ] Connection string configurada corretamente
- [ ] API iniciou sem erros
- [ ] Swagger UI acessível em `http://localhost:5003`
- [ ] Health check retorna "Healthy"
- [ ] Endpoint GET /api/orders funcionando
- [ ] Endpoint GET /api/orders/{id} funcionando
- [ ] Endpoint GET /api/orders/user/{userId} funcionando
- [ ] Endpoint POST /api/orders funcionando
- [ ] Endpoint PUT /api/orders/{id}/status funcionando
- [ ] Endpoint DELETE /api/orders/{id} funcionando
- [ ] OrderNumber sendo gerado automaticamente
- [ ] TotalAmount sendo calculado corretamente
- [ ] Validação de transição de status funcionando
- [ ] Logs sendo gerados
- [ ] Índices criados no MongoDB

## ?? Próximos Passos

Após confirmar que o Ordering.API está funcionando:

1. Integrar com Basket.API para checkout
2. Adicionar autenticação JWT
3. Implementar Event Sourcing para histórico de pedidos
4. Integrar com Event Bus (RabbitMQ) para eventos de pedido
5. Adicionar notificações por email
6. Implementar relatórios e dashboard
7. Adicionar suporte a pagamentos
