# INSTRUÇÕES DE SETUP - Basket.API

## 📋 Pré-requisitos

1. .NET 8 SDK instalado
2. Redis (Docker ou instalação local)
3. Editor de código (Visual Studio, VS Code, ou Rider)

## 🚀 Passo a Passo para Executar

### 1. Navegar até o projeto

```powershell
cd c:\CursoWebApi\projetos\shop-microservices\src\Basket.API
```

### 2. Restaurar dependências

```powershell
dotnet restore
```

### 3. Configurar Redis

**Opção A - Redis via Docker (Recomendado):**

```powershell
docker run -d --name redis -p 6379:6379 redis:alpine
```

**Opção B - Redis com persistência:**

```powershell
docker run -d --name redis -p 6379:6379 -v redis-data:/data redis:alpine redis-server --appendonly yes
```

**Opção C - Alterar connection string:**
Se o Redis estiver em outro host/porta, altere em `appsettings.json`:

```json
"ConnectionStrings": {
  "Redis": "seu-host:6379,password=sua-senha"
}
```

### 4. Executar a aplicação

```powershell
dotnet run
```

### 5. Testar a API

**Swagger UI:**
Abra o navegador em: `http://localhost:5002`

**Health Check:**

```powershell
curl http://localhost:5002/health
```

**Testar endpoints via PowerShell:**

```powershell
# GET - Obter carrinho do usuário (retorna vazio se não existir)
Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123" -Method Get

# POST - Adicionar item ao carrinho
$addItemBody = @{
    productId = 1
    productName = "Laptop"
    price = 999.99
    quantity = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123/items" -Method Post -Body $addItemBody -ContentType "application/json"

# POST - Adicionar outro item
$addItemBody2 = @{
    productId = 2
    productName = "Mouse"
    price = 29.99
    quantity = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123/items" -Method Post -Body $addItemBody2 -ContentType "application/json"

# GET - Ver carrinho atualizado
Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123" -Method Get

# PUT - Atualizar quantidade de um item
$updateQtyBody = @{
    quantity = 5
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123/items/1" -Method Put -Body $updateQtyBody -ContentType "application/json"

# DELETE - Remover item do carrinho
Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123/items/2" -Method Delete

# POST - Criar/atualizar carrinho completo
$basketBody = @{
    userId = "user456"
    items = @(
        @{
            productId = 1
            productName = "Laptop"
            price = 999.99
            quantity = 1
        },
        @{
            productId = 3
            productName = "Keyboard"
            price = 79.99
            quantity = 2
        }
    )
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5002/api/basket" -Method Post -Body $basketBody -ContentType "application/json"

# DELETE - Limpar carrinho completo
Invoke-RestMethod -Uri "http://localhost:5002/api/basket/user123" -Method Delete
```

## 🔧 Comandos Úteis

### Verificar logs

```powershell
# Ver logs em tempo real
Get-Content .\Logs\basket-api-*.log -Wait
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

### Comandos Redis úteis

```powershell
# Conectar ao Redis CLI via Docker
docker exec -it redis redis-cli

# Comandos dentro do redis-cli:
# Ver todas as chaves de basket
KEYS basket:*

# Ver conteúdo de um carrinho específico
GET basket:user123

# Ver TTL de uma chave
TTL basket:user123

# Limpar todo o Redis (CUIDADO!)
FLUSHALL
```

## 📊 Estrutura de Dados

### Basket (Carrinho)

```json
{
  "userId": "user123",
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "price": 999.99,
      "quantity": 2,
      "subtotal": 1999.98
    }
  ],
  "totalPrice": 1999.98
}
```

### Características do Cache

- **TTL**: 24 horas (carrinhos expiram automaticamente)
- **Serialização**: JSON
- **Chave**: `basket:{userId}`

## 🐛 Troubleshooting

### Erro: "Cannot connect to Redis"

- Verifique se o Redis está rodando:
  ```powershell
  docker ps | Select-String redis
  ```
- Teste a conexão:
  ```powershell
  docker exec -it redis redis-cli ping
  # Deve retornar: PONG
  ```
- Confirme a connection string em `appsettings.json`

### Erro: "Redis connection timeout"

- Verifique se a porta 6379 está acessível
- Aumente o timeout na connection string:
  ```json
  "Redis": "localhost:6379,connectTimeout=10000"
  ```

### Porta 5002 já em uso

Altere a porta em `Properties/launchSettings.json`

### Container Redis não inicia

```powershell
# Remover container antigo
docker rm -f redis

# Criar novo
docker run -d --name redis -p 6379:6379 redis:alpine
```

### Limpar cache do NuGet

```powershell
dotnet nuget locals all --clear
```

## 📝 Variáveis de Ambiente

Para ambiente de produção, configure:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__Redis="seu-redis-host:6379,password=sua-senha"
```

## 🔒 Configuração Redis para Produção

Para produção, use uma connection string mais robusta:

```json
"ConnectionStrings": {
  "Redis": "redis-server:6379,password=StrongPassword123,ssl=True,abortConnect=False,connectRetry=3"
}
```

## ✅ Checklist de Validação

- [ ] Redis está rodando
- [ ] Connection string configurada corretamente
- [ ] API iniciou sem erros
- [ ] Swagger UI acessível em `http://localhost:5002`
- [ ] Health check retorna "Healthy"
- [ ] Endpoint GET /api/basket/{userId} funcionando
- [ ] Endpoint POST /api/basket funcionando
- [ ] Endpoint POST /api/basket/{userId}/items funcionando
- [ ] Endpoint PUT /api/basket/{userId}/items/{productId} funcionando
- [ ] Endpoint DELETE /api/basket/{userId}/items/{productId} funcionando
- [ ] Endpoint DELETE /api/basket/{userId} funcionando
- [ ] Logs sendo gerados
- [ ] Dados persistidos no Redis

## 🎯 Próximos Passos

Após confirmar que o Basket.API está funcionando:

1. Integrar com Catalog.API para validar produtos
2. Adicionar autenticação JWT
3. Implementar checkout/order creation
4. Integrar com Event Bus (RabbitMQ) para eventos de carrinho
5. Adicionar cache distribuído para alta disponibilidade
6. Implementar rate limiting
