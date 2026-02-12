# ?? Docker Compose - Shop Microservices

Documentação completa para executar todos os microserviços com Docker Compose.

## ?? Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose v2+ (incluído no Docker Desktop)
- Mínimo 8GB RAM disponível para Docker
- Portas livres: 1433, 5672, 6379, 15672, 27017, 5001, 5002, 5003, 5341

## ?? Quick Start

```powershell
# 1. Copiar arquivo de variáveis de ambiente
cp .env.example .env

# 2. Iniciar toda a infraestrutura e microserviços
docker-compose up -d --build

# 3. Verificar status (aguarde todos ficarem "healthy")
docker-compose ps

# 4. Ver logs em tempo real
docker-compose logs -f
```

## ?? URLs dos Serviços

### Microserviços (Swagger UI)

| Serviço | URL | Descrição |
|---------|-----|-----------|
| Catalog API | http://localhost:5001 | Produtos e Categorias |
| Basket API | http://localhost:5002 | Carrinho de Compras |
| Ordering API | http://localhost:5003 | Gerenciamento de Pedidos |

### Health Checks

| Serviço | Health Endpoint |
|---------|-----------------|
| Catalog API | http://localhost:5001/health |
| Basket API | http://localhost:5002/health |
| Ordering API | http://localhost:5003/health |

### Infraestrutura UI

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| Seq (Logs) | http://localhost:5341 | Sem autenticação |
| RabbitMQ Management | http://localhost:15672 | guest / guest |

### Databases (acesso externo)

| Serviço | Host:Porta | Credenciais |
|---------|-----------|-------------|
| SQL Server | localhost:1433 | sa / YourStrong@Passw0rd123 |
| Redis | localhost:6379 | Sem autenticação |
| MongoDB | localhost:27017 | root / example |

## ?? Arquitetura

```
????????????????????????????????????????????????????????????????????????????
?                            shop-network                                   ?
????????????????????????????????????????????????????????????????????????????
?                                                                           ?
?  ?????????????????   ?????????????????   ?????????????????              ?
?  ?  Catalog API  ?   ?  Basket API   ?   ? Ordering API  ?              ?
?  ?    :5001      ?   ?    :5002      ?   ?    :5003      ?              ?
?  ?  (SQL Server) ?   ?   (Redis)     ?   ?  (MongoDB)    ?              ?
?  ?????????????????   ?????????????????   ?????????????????              ?
?          ?                   ?                   ?                       ?
?          ?                   ?                   ?                       ?
?          ?                   ?                   ?                       ?
?  ?????????????????   ?????????????????   ?????????????????              ?
?  ?  SQL Server   ?   ?     Redis     ?   ?    MongoDB    ?              ?
?  ?    :1433      ?   ?    :6379      ?   ?    :27017     ?              ?
?  ? shop-sqlserver?   ?  shop-redis   ?   ? shop-mongodb  ?              ?
?  ?????????????????   ?????????????????   ?????????????????              ?
?                                                                           ?
?  ?????????????????   ?????????????????                                  ?
?  ?   RabbitMQ    ?   ?      Seq      ? ???? Todos os microserviços     ?
?  ? :5672/:15672  ?   ?    :5341      ?      enviam logs para cá        ?
?  ? shop-rabbitmq ?   ?   shop-seq    ?                                  ?
?  ?????????????????   ?????????????????                                  ?
?                                                                           ?
????????????????????????????????????????????????????????????????????????????
```

## ?? Containers

| Container | Imagem | Porta | Descrição |
|-----------|--------|-------|-----------|
| shop-sqlserver | mcr.microsoft.com/mssql/server:2022-latest | 1433 | SQL Server 2022 Developer |
| shop-redis | redis:7-alpine | 6379 | Redis com AOF persistence |
| shop-mongodb | mongo:6 | 27017 | MongoDB com autenticação |
| shop-rabbitmq | rabbitmq:3-management | 5672, 15672 | Message Broker + UI |
| shop-seq | datalust/seq:latest | 5341 | Logging centralizado |
| shop-catalog-api | Build local | 5001 | .NET 8 API |
| shop-basket-api | Build local | 5002 | .NET 8 API |
| shop-ordering-api | Build local | 5003 | .NET 8 API |

## ?? Comandos Úteis

### Gerenciamento Básico

```powershell
# Iniciar todos os serviços
docker-compose up -d

# Iniciar com rebuild das imagens
docker-compose up -d --build

# Parar todos os serviços (mantém dados)
docker-compose stop

# Parar e remover containers
docker-compose down

# Parar e remover TUDO incluindo volumes (?? APAGA DADOS!)
docker-compose down -v

# Verificar status dos containers
docker-compose ps

# Ver uso de recursos
docker stats
```

### Logs

```powershell
# Ver logs de todos os serviços (tempo real)
docker-compose logs -f

# Logs apenas dos microserviços
docker-compose logs -f catalog-api basket-api ordering-api

# Logs de um serviço específico
docker-compose logs -f catalog-api
docker-compose logs -f basket-api
docker-compose logs -f ordering-api

# Logs da infraestrutura
docker-compose logs -f sqlserver
docker-compose logs -f redis
docker-compose logs -f mongodb
docker-compose logs -f rabbitmq
docker-compose logs -f seq

# Últimas 100 linhas de log
docker-compose logs --tail=100 catalog-api
```

### Rebuild de Serviços

```powershell
# Rebuild apenas um microserviço
docker-compose up -d --build catalog-api
docker-compose up -d --build basket-api
docker-compose up -d --build ordering-api

# Rebuild todos os microserviços de uma vez
docker-compose up -d --build catalog-api basket-api ordering-api

# Forçar rebuild sem cache
docker-compose build --no-cache catalog-api
docker-compose up -d catalog-api
```

### Debug e Acesso aos Containers

```powershell
# Entrar no container (shell)
docker exec -it shop-catalog-api sh
docker exec -it shop-basket-api sh
docker exec -it shop-ordering-api sh

# Ver variáveis de ambiente do container
docker exec shop-catalog-api env | sort

# Testar health check via curl (dentro do container)
docker exec shop-catalog-api curl -s http://localhost:80/health
```

### Acesso aos Databases

```powershell
# SQL Server - conectar via sqlcmd
docker exec -it shop-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd123" -C

# Redis - conectar via redis-cli
docker exec -it shop-redis redis-cli
# Comandos úteis: KEYS *, GET <key>, INFO

# MongoDB - conectar via mongosh
docker exec -it shop-mongodb mongosh -u root -p example
# Comandos úteis: show dbs, use OrderingDb, db.orders.find()
```

## ?? Health Checks

Todos os serviços de infraestrutura possuem health checks configurados. Os microserviços só iniciam quando suas dependências estão "healthy".

### Verificar Status

```powershell
# Ver status de todos os containers
docker-compose ps

# Saída esperada (todos "healthy" ou "running"):
# NAME                COMMAND                  STATUS                   PORTS
# shop-catalog-api    "dotnet Catalog.API.…"   Up 2 minutes (healthy)   0.0.0.0:5001->80/tcp
# shop-basket-api     "dotnet Basket.API.dll"  Up 2 minutes (healthy)   0.0.0.0:5002->8080/tcp
# shop-ordering-api   "dotnet Ordering.API…"   Up 2 minutes (healthy)   0.0.0.0:5003->8080/tcp
# shop-sqlserver      "/opt/mssql/bin/perm…"   Up 3 minutes (healthy)   0.0.0.0:1433->1433/tcp
# shop-redis          "docker-entrypoint.s…"   Up 3 minutes (healthy)   0.0.0.0:6379->6379/tcp
# shop-mongodb        "docker-entrypoint.s…"   Up 3 minutes (healthy)   0.0.0.0:27017->27017/tcp
# shop-rabbitmq       "docker-entrypoint.s…"   Up 3 minutes (healthy)   0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
# shop-seq            "/bin/seq"               Up 3 minutes             0.0.0.0:5341->80/tcp
```

### Testar Health Endpoints

```powershell
# Via PowerShell
Invoke-RestMethod http://localhost:5001/health
Invoke-RestMethod http://localhost:5002/health
Invoke-RestMethod http://localhost:5003/health

# Via curl
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

## ?? Variáveis de Ambiente

### Configuração

```powershell
# Copiar template
cp .env.example .env

# Editar valores (opcional)
notepad .env
```

### Variáveis Disponíveis

| Variável | Default | Descrição |
|----------|---------|-----------|
| SQL_SA_PASSWORD | YourStrong@Passw0rd123 | Senha do SQL Server SA |
| MONGO_PASSWORD | example | Senha do MongoDB root |
| RABBITMQ_USER | guest | Usuário do RabbitMQ |
| RABBITMQ_PASSWORD | guest | Senha do RabbitMQ |
| ASPNETCORE_ENVIRONMENT | Development | Ambiente ASP.NET Core |

### Configuração de Produção

Para produção, use senhas fortes e considere:

```env
SQL_SA_PASSWORD=<senha-complexa-32-caracteres>
MONGO_PASSWORD=<senha-complexa-32-caracteres>
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=<senha-complexa-32-caracteres>
```

## ?? Troubleshooting

### Container não inicia

```powershell
# Ver logs detalhados do container
docker-compose logs <service-name>

# Verificar eventos do container
docker events --filter container=shop-catalog-api
```

### SQL Server demora para iniciar

O SQL Server pode levar 30-60 segundos para ficar "healthy". Os microserviços aguardam automaticamente via `depends_on` com `condition: service_healthy`.

```powershell
# Verificar progresso do SQL Server
docker-compose logs -f sqlserver
```

### Erro de memória

```powershell
# Verificar uso de memória
docker stats --no-stream

# Solução: Aumentar memória no Docker Desktop
# Settings > Resources > Memory (recomendado: 8GB+)
```

### Porta já em uso

```powershell
# Encontrar processo usando a porta (ex: 5001)
netstat -ano | findstr :5001

# Matar processo pelo PID
taskkill /PID <pid> /F

# OU alterar a porta no docker-compose.yml
```

### Limpar tudo e recomeçar

```powershell
# Parar e remover containers + volumes
docker-compose down -v

# Remover imagens antigas dos microserviços
docker image rm shop-microservices-catalog-api shop-microservices-basket-api shop-microservices-ordering-api 2>$null

# Limpar build cache
docker builder prune -f

# Rebuild completo
docker-compose up -d --build
```

### Erro de conexão com banco de dados

```powershell
# Verificar se o banco está acessível
docker exec shop-catalog-api curl -v telnet://sqlserver:1433
docker exec shop-basket-api redis-cli -h redis ping
docker exec shop-ordering-api mongosh --host mongodb --eval "db.runCommand('ping')"
```

### Logs não aparecem no Seq

```powershell
# Verificar se Seq está rodando
docker-compose ps seq

# Verificar conectividade
docker exec shop-catalog-api curl -s http://seq:5341/api

# Verificar configuração no container
docker exec shop-catalog-api env | grep Seq
```

## ?? Monitoramento

### Logs Centralizados (Seq)

Acesse http://localhost:5341 para ver logs de todos os microserviços em uma interface unificada.

Funcionalidades:
- Busca em tempo real
- Filtros por serviço, nível, propriedades
- Dashboard e alertas
- Exportação de logs

### RabbitMQ Management

Acesse http://localhost:15672 (guest/guest) para:
- Monitorar filas e exchanges
- Ver mensagens pending/delivered
- Gerenciar conexões e canais
- Purgar filas (cuidado em produção!)

### Métricas de Containers

```powershell
# Uso de CPU, memória, rede, I/O
docker stats

# Formato tabela customizado
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
```

## ?? Volumes e Persistência

Os dados são persistidos em volumes nomeados:

| Volume | Container | Descrição |
|--------|-----------|-----------|
| shop-sqlserver-data | shop-sqlserver | Databases SQL Server |
| shop-redis-data | shop-redis | Redis AOF persistence |
| shop-mongodb-data | shop-mongodb | MongoDB data files |
| shop-rabbitmq-data | shop-rabbitmq | RabbitMQ mnesia DB |
| shop-seq-data | shop-seq | Seq log storage |

### Gerenciar Volumes

```powershell
# Listar volumes
docker volume ls | findstr shop

# Inspecionar volume
docker volume inspect shop-sqlserver-data

# ?? Remover volume (APAGA DADOS!)
docker volume rm shop-sqlserver-data
```

## ?? Próximos Passos

Após iniciar todos os serviços:

1. ? Verificar health checks de todos os serviços
2. ? Acessar Swagger UI de cada API
3. ? Verificar logs no Seq
4. ? Testar endpoints básicos
5. ?? Aplicar migrations do Catalog.API (se necessário)
6. ?? Configurar API Gateway (Ocelot/YARP)
7. ?? Adicionar autenticação JWT
8. ?? Configurar CI/CD pipeline
