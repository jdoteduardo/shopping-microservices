# INSTRUÇÕES DE SETUP - Catalog.API

## 📋 Pré-requisitos

1. .NET 8 SDK instalado
2. SQL Server (LocalDB, Docker, ou instância completa)
3. Editor de código (Visual Studio, VS Code, ou Rider)

## 🚀 Passo a Passo para Executar

### 1. Navegar até o projeto

```powershell
cd c:\CursoWebApi\projetos\shop-microservices\src\Catalog.API
```

### 2. Restaurar dependências

```powershell
dotnet restore
```

### 3. Configurar SQL Server

**Opção A - SQL Server via Docker (Recomendado):**

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Opção B - Usar LocalDB:**
Altere a connection string em `appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CatalogDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### 4. Criar e aplicar migrations

```powershell
# Instalar EF Core tools (se ainda não tiver)
dotnet tool install --global dotnet-ef

# Criar migration inicial
dotnet ef migrations add InitialCreate

# Aplicar migration ao banco de dados
dotnet ef database update
```

### 5. Executar a aplicação

```powershell
dotnet run
```

### 6. Testar a API

**Swagger UI:**
Abra o navegador em: `http://localhost:5001`

**Health Check:**

```powershell
curl http://localhost:5001/health
```

**Testar endpoints via PowerShell:**

```powershell
# GET - Listar todas as categorias
Invoke-RestMethod -Uri "http://localhost:5001/api/categories" -Method Get

# GET - Listar todos os produtos
Invoke-RestMethod -Uri "http://localhost:5001/api/products" -Method Get

# POST - Criar nova categoria
$categoryBody = @{
    name = "Sports"
    description = "Sports equipment and apparel"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/categories" -Method Post -Body $categoryBody -ContentType "application/json"

# POST - Criar novo produto
$productBody = @{
    name = "Running Shoes"
    description = "Professional running shoes"
    price = 89.99
    stock = 30
    categoryId = 1
    createdBy = "Admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/products" -Method Post -Body $productBody -ContentType "application/json"
```

## 🔧 Comandos Úteis

### Verificar logs

```powershell
# Ver logs em tempo real
Get-Content .\Logs\catalog-api-*.log -Wait
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

### Remover e recriar banco de dados

```powershell
dotnet ef database drop
dotnet ef database update
```

### Criar nova migration

```powershell
dotnet ef migrations add NomeDaMigration
```

## 📊 Dados de Seed

O banco de dados já vem com dados iniciais:

**Categorias:**

1. Electronics
2. Clothing
3. Books

**Produtos:**

1. Laptop (Electronics)
2. T-Shirt (Clothing)
3. C# Programming Book (Books)

## 🐛 Troubleshooting

### Erro: "Cannot connect to SQL Server"

- Verifique se o SQL Server está rodando
- Confirme a connection string em `appsettings.json`
- Teste a conexão com `dotnet ef database update`

### Erro: "dotnet-ef not found"

```powershell
dotnet tool install --global dotnet-ef
```

### Porta 5001 já em uso

Altere a porta em `Properties/launchSettings.json`

### Limpar cache do NuGet

```powershell
dotnet nuget locals all --clear
```

## 📝 Variáveis de Ambiente

Para ambiente de produção, configure:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__DefaultConnection="sua-connection-string-producao"
```

## ✅ Checklist de Validação

- [ ] SQL Server está rodando
- [ ] Connection string configurada corretamente
- [ ] Migrations aplicadas com sucesso
- [ ] API iniciou sem erros
- [ ] Swagger UI acessível
- [ ] Health check retorna "Healthy"
- [ ] Endpoints de produtos funcionando
- [ ] Endpoints de categorias funcionando
- [ ] Logs sendo gerados

## 🎯 Próximos Passos

Após confirmar que o Catalog.API está funcionando:

1. Criar Dockerfile para containerização
2. Adicionar autenticação JWT
3. Implementar paginação nos endpoints GET
4. Adicionar cache com Redis
5. Integrar com Event Bus (RabbitMQ)
