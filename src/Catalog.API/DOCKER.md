# Catalog.API - Docker Instructions

## 🐳 Executar com Docker Compose

### Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose habilitado

### Executar tudo (API + SQL Server)

```powershell
# Navegar até o diretório
cd c:\CursoWebApi\projetos\shop-microservices\src\Catalog.API

# Build e start dos containers
docker-compose up -d --build

# Verificar status dos containers
docker-compose ps

# Ver logs
docker-compose logs -f

# Apenas logs da API
docker-compose logs -f catalog-api

# Apenas logs do banco
docker-compose logs -f catalog-db
```

### Aplicar Migrations no container

```powershell
# Esperar o SQL Server estar pronto (pode levar 30-60 segundos)
Start-Sleep -Seconds 60

# Executar migrations dentro do container
docker-compose exec catalog-api dotnet ef database update
```

OU criar as migrations localmente primeiro:

```powershell
# Na máquina local (antes do docker-compose up)
dotnet ef migrations add InitialCreate

# Depois fazer o build e up
docker-compose up -d --build

# O container aplicará automaticamente ou você pode executar:
docker-compose exec catalog-api dotnet ef database update
```

### Testar a API

```powershell
# Health check
curl http://localhost:5001/health

# Swagger UI
# Abrir no navegador: http://localhost:5001

# Listar produtos
Invoke-RestMethod -Uri "http://localhost:5001/api/products" -Method Get

# Listar categorias
Invoke-RestMethod -Uri "http://localhost:5001/api/categories" -Method Get
```

### Parar e remover containers

```powershell
# Parar containers
docker-compose stop

# Parar e remover containers
docker-compose down

# Remover containers, networks E volumes (apaga o banco!)
docker-compose down -v
```

## 🔨 Build apenas da imagem Docker

```powershell
# Build da imagem
docker build -t catalog-api:latest .

# Executar apenas a API (assumindo SQL Server já rodando)
docker run -d `
  --name catalog-api `
  -p 5001:80 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=CatalogDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" `
  catalog-api:latest
```

## 🔍 Comandos de Debug

```powershell
# Entrar no container da API
docker-compose exec catalog-api bash

# Entrar no container do SQL Server
docker-compose exec catalog-db bash

# Ver variáveis de ambiente
docker-compose exec catalog-api env

# Verificar conectividade do banco
docker-compose exec catalog-api dotnet ef dbcontext info
```

## 📊 Monitoramento

```powershell
# Ver uso de recursos
docker stats

# Inspecionar container
docker inspect catalog-api

# Ver networks
docker network ls
docker network inspect catalog-api_catalog-network
```

## 🔄 Rebuild após mudanças no código

```powershell
# Parar, rebuild e reiniciar
docker-compose down
docker-compose up -d --build

# Ou apenas rebuild um serviço específico
docker-compose up -d --build catalog-api
```

## 💾 Gerenciamento de Volumes

```powershell
# Listar volumes
docker volume ls

# Inspecionar volume do banco
docker volume inspect catalog-api_catalog-db-data

# Backup do banco (opcional)
docker-compose exec catalog-db /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "YourStrong@Passw0rd" `
  -Q "BACKUP DATABASE [CatalogDb] TO DISK = N'/var/opt/mssql/backup/CatalogDb.bak'"
```

## 🌐 Acessar de outras máquinas na rede

Se quiser acessar a API de outras máquinas na rede local:

1. Descubra o IP da sua máquina:

```powershell
ipconfig
```

2. Acesse usando:

```
http://SEU_IP:5001
```

3. Garanta que o firewall permite conexões na porta 5001

## ⚙️ Configurações de Produção

Para produção, crie um `docker-compose.prod.yml`:

```yaml
version: "3.8"

services:
  catalog-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
    restart: always
    deploy:
      resources:
        limits:
          cpus: "1"
          memory: 512M
```

Executar:

```powershell
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 🐛 Troubleshooting

### Container da API não inicia

- Verifique se o SQL Server está healthy: `docker-compose ps`
- Veja os logs: `docker-compose logs catalog-api`
- Aguarde mais tempo para o SQL Server inicializar

### Erro de conexão com banco

- Verifique a connection string
- Confirme que o SQL Server está no estado "healthy"
- Verifique se os containers estão na mesma network

### Porta já em uso

- Altere a porta em `docker-compose.yml`: `"5002:80"`
- Ou pare o processo usando a porta 5001

### Migrations não aplicadas

```powershell
# Aplicar manualmente
docker-compose exec catalog-api dotnet ef database update
```
