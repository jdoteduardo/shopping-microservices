# 📊 ShopMicroservices — Progresso do Projeto

> Acompanhamento de todas as tarefas do projeto, da fundação ao deploy.
> Última atualização: 06/03/2026

---

## 📆 SEMANA 1: FUNDAÇÃO (3 APIs + Docker + Testes + Docs)

- [x] Estrutura inicial do projeto (solution, pastas, .gitignore)
- [x] **Catalog.API** — CRUD de produtos, EF Core, SQL Server, FluentValidation
- [x] **Basket.API** — Carrinho de compras, Redis
- [x] **Ordering.API** — Gerenciamento de pedidos, MongoDB
- [x] **Docker Compose** — Orquestração de todos os serviços + infraestrutura
- [x] **Testes unitários** — Projetos de teste para cada API
- [x] **Documentação inicial** — README.md básico

---

## 📆 SEMANA 2: INTEGRAÇÃO (Mensageria + Auth + Gateway)

### DIA 8: EventBus + IntegrationEvents
- [x] **Tarefa 8.1** — Criar biblioteca `EventBus` (abstrações)
- [x] **Tarefa 8.2** — Criar biblioteca `EventBus.RabbitMQ` (implementação)

### DIA 9: Pub/Sub nos Microserviços
- [x] **Tarefa 9.1** — Ordering.API como Publisher (OrderCreated, OrderStatusChanged)
- [x] **Tarefa 9.2** — Catalog.API como Subscriber (decrementar estoque no pedido)

### DIA 10: Autenticação JWT
- [x] **Tarefa 10.1** — Criar biblioteca Auth (JWT, BCrypt, mock users)
- [x] **Tarefa 10.2** — Criar biblioteca UserContext (headers X-User-Id, X-User-Email, X-User-Roles)

### DIA 11: API Gateway
- [x] **Tarefa 11.1** — Criar ApiGateway com Ocelot (rotas, JWT, CORS, rate limiting)
- [x] **Tarefa 11.2** — Atualizar docker-compose.yml (gateway na porta 5000, remover portas externas das APIs)

### DIA 12: Integrar UserContext
- [x] **Tarefa 12.1** — Catalog.API — UserContext + role-based authorization
- [x] **Tarefa 12.2** — Ordering.API — UserContext + ownership de pedidos
- [x] **Tarefa 12.3** — Basket.API — UserContext + carrinho por usuário

### DIA 13: Testes + Documentação
- [x] **Tarefa 13.1** — Unit tests Auth (JwtTokenGeneratorTests)
- [x] **Tarefa 13.2** — Script de testes e2e atualizado com fluxo JWT

### DIA 14: Documentação + Refinamento
- [x] **Tarefa 14.1** — `docs/AUTHENTICATION.md`
- [x] **Tarefa 14.2** — Update `README.md` com seções de auth/gateway
- [x] **Tarefa 14.3** — Postman Collection `tests/ShopMicroservices.postman_collection.json`

---

## 📆 SEMANA 3: OBSERVABILIDADE + CI/CD

### DIA 15–16: Observability Stack
- [x] **Tarefa 15.1** — Instrumentar microserviços com OpenTelemetry (tracing + métricas + `/metrics` endpoint)
- [x] **Tarefa 15.2** — Setup Prometheus + Grafana (prometheus.yml, dashboards, docker-compose)

### DIA 17–18: GitHub Actions CI/CD
- [ ] **Tarefa 17.1** — CI Pipeline: Build + Test (`.github/workflows/ci.yml`)
- [ ] **Tarefa 17.2** — CD Pipeline: Docker Build + Push (`.github/workflows/cd.yml` + `deploy-proxmox.yml`)

### DIA 19: Health Checks + Alerting
- [ ] **Tarefa 19.1** — Grafana Alerting (service down, error rate, response time, queue depth)
- [ ] **Tarefa 19.2** — Centralized Logging Improvements (Seq + correlation IDs)

### DIA 20–21: Integration Tests + E2E
- [ ] **Tarefa 20.1** — Docker Compose Integration Tests (`.github/workflows/integration-tests.yml`)
- [ ] **Tarefa 20.2** — Unit Tests Expansion (cobertura em todos os projetos de teste)

---

## 📆 SEMANA 4: DEPLOY PROXMOX + FINALIZAÇÃO

### DIA 22: Setup VM Proxmox
- [ ] **Tarefa 22.1** — Criar VM no Proxmox (Ubuntu 22.04, 4 cores, 8GB RAM)
- [ ] **Tarefa 22.2** — Script de setup da VM (`deploy/proxmox/setup-vm.sh`)

### DIA 23: Deploy Scripts + Docker Compose Produção
- [ ] **Tarefa 23.1** — `docker-compose.prod.yml` + `deploy/proxmox/deploy.sh` + `.env.prod.example`
- [ ] **Tarefa 23.2** — Nginx Reverse Proxy (`deploy/proxmox/nginx/nginx.conf`)

### DIA 24: Backup + Manutenção
- [ ] **Tarefa 24.1** — Scripts de backup e restore (MongoDB, SQL Server, Redis)
- [ ] **Tarefa 24.2** — Cron jobs + monitoramento (`deploy/proxmox/setup-cron.sh`)

### DIA 25–26: Documentação Final
- [ ] **Tarefa 25.1** — `docs/DEPLOYMENT.md` (guia Proxmox)
- [ ] **Tarefa 25.2** — `docs/CI-CD.md` (workflows GitHub Actions)
- [ ] **Tarefa 25.3** — `docs/MONITORING.md` (Grafana, Prometheus, Seq)
- [ ] **Tarefa 25.4** — Update final do `README.md`

### DIA 27–28: Testes Finais + Checkpoint
- [ ] **Tarefa 27.1** — Full Stack Validation (docker-compose, Prometheus, Grafana, CI, CD)
- [ ] **Tarefa 27.2** — Final Commit + Tag `v1.0.0`

---

## 📈 Resumo de Progresso

| Semana | Foco | Status | Progresso |
|--------|------|--------|-----------|
| Semana 1 | Foundation (3 APIs + Docker) | ✅ Concluída | 100% |
| Semana 2 | Integration (Pub/Sub + Auth + Gateway) | ✅ Concluída | 100% |
| Semana 3 | Observability + CI/CD | 🔲 Não iniciada | 0% |
| Semana 4 | Proxmox Deploy + Finalização | 🔲 Não iniciada | 0% |

**Progresso geral: ~50%** (Semanas 1–2 completas, Semanas 3–4 pendentes)
