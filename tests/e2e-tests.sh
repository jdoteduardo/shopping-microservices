#!/bin/bash

# ============================================================================
# Shop Microservices - End-to-End Tests
# ============================================================================
# Script de testes end-to-end para validar os microserviços
#
# USO:
#   chmod +x tests/e2e-tests.sh
#   ./tests/e2e-tests.sh
#
# PRÉ-REQUISITOS:
#   - Docker Compose rodando com todos os serviços healthy
#   - curl instalado
#   - jq instalado (opcional, para parsing JSON)
#
# PORTAS:
#   - Catalog API: 5001
#   - Basket API:  5002
#   - Ordering API: 5003
# ============================================================================

set -e

# ============================================================================
# CONFIGURAÇÃO
# ============================================================================

# URLs das APIs
CATALOG_API="http://localhost:5001"
BASKET_API="http://localhost:5002"
ORDERING_API="http://localhost:5003"

# Gerar sufixo único para evitar conflitos de duplicidade
TEST_SUFFIX=$(date +%Y%m%d%H%M%S)

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color
BOLD='\033[1m'

# Contadores de testes
TESTS_PASSED=0
TESTS_FAILED=0
TESTS_TOTAL=0

# Variáveis para armazenar IDs criados
CATEGORY_ID=""
PRODUCT_LAPTOP_ID=""
PRODUCT_MOUSE_ID=""
ORDER_ID=""

# Delay entre requests (ms)
DELAY_MS=500

# ============================================================================
# FUNÇÕES AUXILIARES
# ============================================================================

# Função para delay
delay() {
    sleep $(echo "scale=3; $DELAY_MS/1000" | bc)
}

# Função para imprimir cabeçalho
print_header() {
    echo ""
    echo -e "${BLUE}???????????????????????????????????????????????????????????????????${NC}"
    echo -e "${BOLD}${CYAN}  $1${NC}"
    echo -e "${BLUE}???????????????????????????????????????????????????????????????????${NC}"
    echo ""
}

# Função para imprimir subheader
print_subheader() {
    echo ""
    echo -e "${YELLOW}???????????????????????????????????????????????????????????????????${NC}"
    echo -e "${BOLD}  $1${NC}"
    echo -e "${YELLOW}???????????????????????????????????????????????????????????????????${NC}"
}

# Função para imprimir teste
print_test() {
    echo -e "${CYAN}? TEST:${NC} $1"
}

# Função para imprimir sucesso
print_success() {
    echo -e "${GREEN}  ? PASS:${NC} $1"
    ((TESTS_PASSED++))
    ((TESTS_TOTAL++))
}

# Função para imprimir falha
print_fail() {
    echo -e "${RED}  ? FAIL:${NC} $1"
    ((TESTS_FAILED++))
    ((TESTS_TOTAL++))
}

# Função para imprimir info
print_info() {
    echo -e "${BLUE}  ??  INFO:${NC} $1"
}

# Função para imprimir warning
print_warning() {
    echo -e "${YELLOW}  ??  WARN:${NC} $1"
}

# Função para fazer request e validar status code
# Uso: make_request "METHOD" "URL" "BODY" "EXPECTED_STATUS" "DESCRIPTION"
make_request() {
    local method=$1
    local url=$2
    local body=$3
    local expected_status=$4
    local description=$5
    
    print_test "$description"
    
    if [ -z "$body" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$url" \
            -H "Content-Type: application/json" 2>/dev/null)
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -d "$body" 2>/dev/null)
    fi
    
    # Separar body e status code
    http_code=$(echo "$response" | tail -n1)
    body_response=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -eq "$expected_status" ]; then
        print_success "HTTP $http_code (expected $expected_status)"
        echo "$body_response"
        delay
        return 0
    else
        print_fail "HTTP $http_code (expected $expected_status)"
        echo -e "${RED}Response: $body_response${NC}"
        delay
        return 1
    fi
}

# Função para extrair valor JSON (sem jq)
extract_json_value() {
    local json=$1
    local key=$2
    echo "$json" | grep -o "\"$key\":[^,}]*" | sed "s/\"$key\"://" | tr -d '"' | tr -d ' '
}

# Função para extrair ID de resposta JSON
extract_id() {
    local json=$1
    # Tenta extrair "id" primeiro, depois "_id"
    local id=$(echo "$json" | grep -o '"id":[^,}]*' | head -1 | sed 's/"id"://' | tr -d '"' | tr -d ' ')
    if [ -z "$id" ]; then
        id=$(echo "$json" | grep -o '"_id":[^,}]*' | head -1 | sed 's/"_id"://' | tr -d '"' | tr -d ' ')
    fi
    echo "$id"
}

# ============================================================================
# VERIFICAÇÃO DE SAÚDE DOS SERVIÇOS
# ============================================================================

check_services_health() {
    print_header "?? VERIFICAÇÃO DE SAÚDE DOS SERVIÇOS"
    
    local all_healthy=true
    
    # Catalog API
    print_test "Catalog API Health Check"
    if curl -s -f "$CATALOG_API/health" > /dev/null 2>&1; then
        print_success "Catalog API está healthy"
    else
        print_fail "Catalog API não está respondendo"
        all_healthy=false
    fi
    delay
    
    # Basket API
    print_test "Basket API Health Check"
    if curl -s -f "$BASKET_API/health" > /dev/null 2>&1; then
        print_success "Basket API está healthy"
    else
        print_fail "Basket API não está respondendo"
        all_healthy=false
    fi
    delay
    
    # Ordering API
    print_test "Ordering API Health Check"
    if curl -s -f "$ORDERING_API/health" > /dev/null 2>&1; then
        print_success "Ordering API está healthy"
    else
        print_fail "Ordering API não está respondendo"
        all_healthy=false
    fi
    delay
    
    if [ "$all_healthy" = false ]; then
        echo ""
        echo -e "${RED}???????????????????????????????????????????????????????????????????${NC}"
        echo -e "${RED}  ? ERRO: Nem todos os serviços estão healthy!${NC}"
        echo -e "${RED}  Execute: docker-compose ps${NC}"
        echo -e "${RED}???????????????????????????????????????????????????????????????????${NC}"
        exit 1
    fi
}

# ============================================================================
# CENÁRIO 1: FLUXO COMPLETO DE COMPRA
# ============================================================================

scenario_1_complete_purchase_flow() {
    print_header "?? CENÁRIO 1: FLUXO COMPLETO DE COMPRA"
    
    # -------------------------------------------------------------------------
    # 1.1 Criar categoria com nome único
    # -------------------------------------------------------------------------
    print_subheader "1.1 Criar categoria 'Electronics-$TEST_SUFFIX'"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$CATALOG_API/api/categories" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"Electronics-$TEST_SUFFIX\",
            \"description\": \"Electronic devices and gadgets - Test $TEST_SUFFIX\"
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar categoria Electronics-$TEST_SUFFIX"
    if [ "$http_code" -eq 201 ]; then
        CATEGORY_ID=$(extract_id "$body")
        print_success "Categoria criada com ID: $CATEGORY_ID"
        print_info "Response: $body"
    else
        print_fail "Falha ao criar categoria (HTTP $http_code)"
        print_info "Response: $body"
        # Fallback para categoria existente
        CATEGORY_ID="1"
        print_warning "Usando categoria padrão ID: 1"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.2 Criar produto "Laptop" com nome único
    # -------------------------------------------------------------------------
    print_subheader "1.2 Criar produto 'Laptop-$TEST_SUFFIX'"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$CATALOG_API/api/products" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"Laptop Gamer $TEST_SUFFIX\",
            \"description\": \"High-performance gaming laptop\",
            \"price\": 1999.99,
            \"stock\": 50,
            \"categoryId\": $CATEGORY_ID,
            \"createdBy\": \"e2e-test\"
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar produto Laptop"
    if [ "$http_code" -eq 201 ]; then
        PRODUCT_LAPTOP_ID=$(extract_id "$body")
        print_success "Produto criado com ID: $PRODUCT_LAPTOP_ID"
        print_info "Response: $body"
    else
        print_fail "Falha ao criar produto Laptop (HTTP $http_code)"
        print_info "Response: $body"
        PRODUCT_LAPTOP_ID="1"
        print_warning "Usando produto padrão ID: 1"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.3 Criar produto "Mouse" com nome único
    # -------------------------------------------------------------------------
    print_subheader "1.3 Criar produto 'Mouse-$TEST_SUFFIX'"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$CATALOG_API/api/products" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"Gaming Mouse $TEST_SUFFIX\",
            \"description\": \"RGB gaming mouse with 16000 DPI\",
            \"price\": 79.99,
            \"stock\": 100,
            \"categoryId\": $CATEGORY_ID,
            \"createdBy\": \"e2e-test\"
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar produto Mouse"
    if [ "$http_code" -eq 201 ]; then
        PRODUCT_MOUSE_ID=$(extract_id "$body")
        print_success "Produto criado com ID: $PRODUCT_MOUSE_ID"
        print_info "Response: $body"
    else
        print_fail "Falha ao criar produto Mouse (HTTP $http_code)"
        print_info "Response: $body"
        PRODUCT_MOUSE_ID="2"
        print_warning "Usando produto padrão ID: 2"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.4 Listar todos os produtos
    # -------------------------------------------------------------------------
    print_subheader "1.4 Listar todos os produtos"
    
    response=$(curl -s -w "\n%{http_code}" -X GET "$CATALOG_API/api/products" \
        -H "Content-Type: application/json" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Listar produtos"
    if [ "$http_code" -eq 200 ]; then
        print_success "Produtos listados com sucesso"
        print_info "Response: $body"
    else
        print_fail "Falha ao listar produtos (HTTP $http_code)"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.5 Adicionar Laptop ao carrinho
    # -------------------------------------------------------------------------
    print_subheader "1.5 Adicionar Laptop ao carrinho do user123-$TEST_SUFFIX"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$BASKET_API/api/basket/user123-$TEST_SUFFIX/items" \
        -H "Content-Type: application/json" \
        -d "{
            \"productId\": $PRODUCT_LAPTOP_ID,
            \"productName\": \"Laptop Gamer $TEST_SUFFIX\",
            \"price\": 1999.99,
            \"quantity\": 1
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Adicionar Laptop ao carrinho"
    if [ "$http_code" -eq 200 ]; then
        print_success "Laptop adicionado ao carrinho"
        print_info "Response: $body"
    else
        print_fail "Falha ao adicionar Laptop (HTTP $http_code)"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.6 Adicionar Mouse ao carrinho
    # -------------------------------------------------------------------------
    print_subheader "1.6 Adicionar Mouse ao carrinho do user123-$TEST_SUFFIX"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$BASKET_API/api/basket/user123-$TEST_SUFFIX/items" \
        -H "Content-Type: application/json" \
        -d "{
            \"productId\": $PRODUCT_MOUSE_ID,
            \"productName\": \"Gaming Mouse $TEST_SUFFIX\",
            \"price\": 79.99,
            \"quantity\": 2
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Adicionar Mouse ao carrinho"
    if [ "$http_code" -eq 200 ]; then
        print_success "Mouse adicionado ao carrinho"
        print_info "Response: $body"
    else
        print_fail "Falha ao adicionar Mouse (HTTP $http_code)"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.7 Obter carrinho e validar total
    # -------------------------------------------------------------------------
    print_subheader "1.7 Obter carrinho do user123-$TEST_SUFFIX"
    
    response=$(curl -s -w "\n%{http_code}" -X GET "$BASKET_API/api/basket/user123-$TEST_SUFFIX" \
        -H "Content-Type: application/json" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Obter carrinho"
    if [ "$http_code" -eq 200 ]; then
        print_success "Carrinho obtido com sucesso"
        print_info "Response: $body"
        
        # Validar total (1999.99 + 79.99*2 = 2159.97)
        if echo "$body" | grep -q "2159.97"; then
            print_success "Total do carrinho correto: 2159.97"
        else
            print_warning "Total do carrinho pode estar incorreto"
        fi
    else
        print_fail "Falha ao obter carrinho (HTTP $http_code)"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.8 Criar pedido com itens do carrinho
    # -------------------------------------------------------------------------
    print_subheader "1.8 Criar pedido"
    
    print_info "Criando pedido com ProductLaptopId=$PRODUCT_LAPTOP_ID, ProductMouseId=$PRODUCT_MOUSE_ID"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$ORDERING_API/api/orders" \
        -H "Content-Type: application/json" \
        -d "{
            \"userId\": \"user123-$TEST_SUFFIX\",
            \"items\": [
                {
                    \"productId\": $PRODUCT_LAPTOP_ID,
                    \"productName\": \"Laptop Gamer $TEST_SUFFIX\",
                    \"price\": 1999.99,
                    \"quantity\": 1
                },
                {
                    \"productId\": $PRODUCT_MOUSE_ID,
                    \"productName\": \"Gaming Mouse $TEST_SUFFIX\",
                    \"price\": 79.99,
                    \"quantity\": 2
                }
            ],
            \"shippingAddress\": {
                \"street\": \"123 Main Street\",
                \"city\": \"São Paulo\",
                \"state\": \"SP\",
                \"zipCode\": \"01310-100\",
                \"country\": \"Brazil\"
            }
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar pedido"
    if [ "$http_code" -eq 201 ]; then
        ORDER_ID=$(extract_id "$body")
        print_success "Pedido criado com ID: $ORDER_ID"
        print_info "Response: $body"
    else
        print_fail "Falha ao criar pedido (HTTP $http_code)"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.9 Verificar pedido criado
    # -------------------------------------------------------------------------
    print_subheader "1.9 Verificar pedido criado"
    
    if [ -n "$ORDER_ID" ] && [ "$ORDER_ID" != "null" ]; then
        response=$(curl -s -w "\n%{http_code}" -X GET "$ORDERING_API/api/orders/$ORDER_ID" \
            -H "Content-Type: application/json" 2>/dev/null)
        
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')
        
        print_test "Obter pedido por ID"
        if [ "$http_code" -eq 200 ]; then
            print_success "Pedido obtido com sucesso"
            print_info "Response: $body"
        else
            print_fail "Falha ao obter pedido (HTTP $http_code)"
        fi
    else
        print_warning "ORDER_ID não disponível - pulando teste"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 1.10 Atualizar status do pedido para "Confirmed"
    # -------------------------------------------------------------------------
    print_subheader "1.10 Atualizar status para 'Confirmed'"
    
    if [ -n "$ORDER_ID" ] && [ "$ORDER_ID" != "null" ]; then
        response=$(curl -s -w "\n%{http_code}" -X PUT "$ORDERING_API/api/orders/$ORDER_ID/status" \
            -H "Content-Type: application/json" \
            -d '{
                "status": 1
            }' 2>/dev/null)
        
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')
        
        print_test "Atualizar status para Confirmed"
        if [ "$http_code" -eq 200 ]; then
            print_success "Status atualizado para Confirmed"
            print_info "Response: $body"
        else
            print_fail "Falha ao atualizar status (HTTP $http_code)"
            print_info "Response: $body"
        fi
    else
        print_warning "ORDER_ID não disponível - pulando teste"
    fi
    delay
}

# ============================================================================
# CENÁRIO 2: VALIDAÇÕES
# ============================================================================

scenario_2_validations() {
    print_header "?? CENÁRIO 2: VALIDAÇÕES"
    
    # -------------------------------------------------------------------------
    # 2.1 Tentar criar produto com dados inválidos
    # -------------------------------------------------------------------------
    print_subheader "2.1 Criar produto com dados inválidos"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$CATALOG_API/api/products" \
        -H "Content-Type: application/json" \
        -d '{
            "name": "",
            "description": "Test",
            "price": -10,
            "stock": -5,
            "categoryId": 0,
            "createdBy": ""
        }' 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar produto com dados inválidos (deve falhar)"
    if [ "$http_code" -eq 400 ]; then
        print_success "Requisição rejeitada corretamente (HTTP 400)"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 400, recebeu HTTP $http_code"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 2.2 Tentar criar produto sem nome
    # -------------------------------------------------------------------------
    print_subheader "2.2 Criar produto sem nome"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$CATALOG_API/api/products" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"\",
            \"description\": \"Test product\",
            \"price\": 99.99,
            \"stock\": 10,
            \"categoryId\": $CATEGORY_ID,
            \"createdBy\": \"test\"
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar produto sem nome (deve falhar)"
    if [ "$http_code" -eq 400 ]; then
        print_success "Validação de nome funcionando (HTTP 400)"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 400, recebeu HTTP $http_code"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 2.3 Tentar adicionar item com quantidade negativa
    # -------------------------------------------------------------------------
    print_subheader "2.3 Adicionar item com quantidade negativa"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$BASKET_API/api/basket/usertest/items" \
        -H "Content-Type: application/json" \
        -d '{
            "productId": 1,
            "productName": "Test",
            "price": 10.00,
            "quantity": -5
        }' 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Adicionar item com quantidade negativa (deve falhar)"
    if [ "$http_code" -eq 400 ]; then
        print_success "Validação de quantidade funcionando (HTTP 400)"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 400, recebeu HTTP $http_code"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 2.4 Tentar criar pedido vazio
    # -------------------------------------------------------------------------
    print_subheader "2.4 Criar pedido sem itens"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$ORDERING_API/api/orders" \
        -H "Content-Type: application/json" \
        -d '{
            "userId": "usertest",
            "items": [],
            "shippingAddress": {
                "street": "Test Street",
                "city": "Test City",
                "state": "TS",
                "zipCode": "12345",
                "country": "Test"
            }
        }' 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar pedido sem itens (deve falhar)"
    if [ "$http_code" -eq 400 ]; then
        print_success "Validação de itens funcionando (HTTP 400)"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 400, recebeu HTTP $http_code"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 2.5 Tentar criar pedido sem userId
    # -------------------------------------------------------------------------
    print_subheader "2.5 Criar pedido sem userId"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$ORDERING_API/api/orders" \
        -H "Content-Type: application/json" \
        -d '{
            "userId": "",
            "items": [
                {
                    "productId": 1,
                    "productName": "Test",
                    "price": 10.00,
                    "quantity": 1
                }
            ],
            "shippingAddress": {
                "street": "Test Street",
                "city": "Test City",
                "state": "TS",
                "zipCode": "12345",
                "country": "Test"
            }
        }' 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Criar pedido sem userId (deve falhar)"
    if [ "$http_code" -eq 400 ]; then
        print_success "Validação de userId funcionando (HTTP 400)"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 400, recebeu HTTP $http_code"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 2.6 Tentar obter pedido inexistente
    # -------------------------------------------------------------------------
    print_subheader "2.6 Obter pedido inexistente"
    
    response=$(curl -s -w "\n%{http_code}" -X GET "$ORDERING_API/api/orders/000000000000000000000000" \
        -H "Content-Type: application/json" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Obter pedido inexistente (deve retornar 404)"
    if [ "$http_code" -eq 404 ]; then
        print_success "Pedido não encontrado retorna 404 corretamente"
        print_info "Response: $body"
    else
        print_fail "Esperado HTTP 404, recebeu HTTP $http_code"
    fi
    delay
}

# ============================================================================
# CENÁRIO 3: LIMPEZA
# ============================================================================

scenario_3_cleanup() {
    print_header "?? CENÁRIO 3: LIMPEZA"
    
    # -------------------------------------------------------------------------
    # 3.1 Limpar carrinho
    # -------------------------------------------------------------------------
    print_subheader "3.1 Limpar carrinho do user123-$TEST_SUFFIX"
    
    response=$(curl -s -w "\n%{http_code}" -X DELETE "$BASKET_API/api/basket/user123-$TEST_SUFFIX" \
        -H "Content-Type: application/json" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Limpar carrinho"
    if [ "$http_code" -eq 204 ]; then
        print_success "Carrinho limpo com sucesso"
    elif [ "$http_code" -eq 404 ]; then
        print_success "Carrinho já estava vazio (HTTP 404)"
    else
        print_fail "Falha ao limpar carrinho (HTTP $http_code)"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 3.2 Verificar que carrinho está vazio
    # -------------------------------------------------------------------------
    print_subheader "3.2 Verificar carrinho vazio"
    
    response=$(curl -s -w "\n%{http_code}" -X GET "$BASKET_API/api/basket/user123-$TEST_SUFFIX" \
        -H "Content-Type: application/json" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    print_test "Verificar carrinho vazio"
    if [ "$http_code" -eq 200 ]; then
        if echo "$body" | grep -q '"items":\[\]' || echo "$body" | grep -q '"totalPrice":0'; then
            print_success "Carrinho está vazio"
        else
            print_warning "Carrinho pode não estar vazio"
        fi
        print_info "Response: $body"
    else
        print_info "Carrinho retornou HTTP $http_code"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 3.3 Criar novo pedido para cancelamento
    # -------------------------------------------------------------------------
    print_subheader "3.3 Criar pedido para teste de cancelamento"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$ORDERING_API/api/orders" \
        -H "Content-Type: application/json" \
        -d "{
            \"userId\": \"user-cleanup-$TEST_SUFFIX\",
            \"items\": [
                {
                    \"productId\": 1,
                    \"productName\": \"Test Product for Cancel\",
                    \"price\": 10.00,
                    \"quantity\": 1
                }
            ],
            \"shippingAddress\": {
                \"street\": \"Test Street\",
                \"city\": \"Test City\",
                \"state\": \"TS\",
                \"zipCode\": \"12345\",
                \"country\": \"Test\"
            }
        }" 2>/dev/null)
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    local cancel_order_id=""
    
    print_test "Criar pedido para cancelamento"
    if [ "$http_code" -eq 201 ]; then
        cancel_order_id=$(extract_id "$body")
        print_success "Pedido criado com ID: $cancel_order_id"
        print_info "Response: $body"
    else
        print_fail "Falha ao criar pedido (HTTP $http_code)"
        print_info "Response: $body"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 3.4 Cancelar pedido
    # -------------------------------------------------------------------------
    print_subheader "3.4 Cancelar pedido"
    
    if [ -n "$cancel_order_id" ] && [ "$cancel_order_id" != "null" ]; then
        response=$(curl -s -w "\n%{http_code}" -X DELETE "$ORDERING_API/api/orders/$cancel_order_id" \
            -H "Content-Type: application/json" 2>/dev/null)
        
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')
        
        print_test "Cancelar pedido"
        if [ "$http_code" -eq 204 ]; then
            print_success "Pedido cancelado com sucesso"
        else
            print_fail "Falha ao cancelar pedido (HTTP $http_code)"
            print_info "Response: $body"
        fi
    else
        print_warning "ID do pedido não disponível - pulando teste"
    fi
    delay
    
    # -------------------------------------------------------------------------
    # 3.5 Verificar que pedido foi cancelado
    # -------------------------------------------------------------------------
    print_subheader "3.5 Verificar status do pedido cancelado"
    
    if [ -n "$cancel_order_id" ] && [ "$cancel_order_id" != "null" ]; then
        response=$(curl -s -w "\n%{http_code}" -X GET "$ORDERING_API/api/orders/$cancel_order_id" \
            -H "Content-Type: application/json" 2>/dev/null)
        
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')
        
        print_test "Verificar status Cancelled"
        if [ "$http_code" -eq 200 ]; then
            if echo "$body" | grep -qi "cancelled\|cancel"; then
                print_success "Pedido está com status Cancelled"
            else
                print_warning "Status do pedido pode não ser Cancelled"
            fi
            print_info "Response: $body"
        else
            print_fail "Falha ao obter pedido (HTTP $http_code)"
        fi
    else
        print_warning "ID do pedido não disponível - pulando teste"
    fi
    delay
}

# ============================================================================
# SUMMARY
# ============================================================================

print_summary() {
    echo ""
    echo -e "${BLUE}???????????????????????????????????????????????????????????????????${NC}"
    echo -e "${BOLD}${CYAN}  ?? RESUMO DOS TESTES${NC}"
    echo -e "${BLUE}???????????????????????????????????????????????????????????????????${NC}"
    echo ""
    echo -e "  ${GREEN}? Testes Passaram:${NC}  $TESTS_PASSED"
    echo -e "  ${RED}? Testes Falharam:${NC}  $TESTS_FAILED"
    echo -e "  ${BLUE}?? Total de Testes:${NC}  $TESTS_TOTAL"
    echo ""
    
    if [ $TESTS_FAILED -eq 0 ]; then
        echo -e "${GREEN}???????????????????????????????????????????????????????????????????${NC}"
        echo -e "${GREEN}  ?? TODOS OS TESTES PASSARAM!${NC}"
        echo -e "${GREEN}???????????????????????????????????????????????????????????????????${NC}"
    else
        echo -e "${RED}???????????????????????????????????????????????????????????????????${NC}"
        echo -e "${RED}  ??  ALGUNS TESTES FALHARAM!${NC}"
        echo -e "${RED}???????????????????????????????????????????????????????????????????${NC}"
    fi
    echo ""
    
    # Informações adicionais
    echo -e "${YELLOW}???????????????????????????????????????????????????????????????????${NC}"
    echo -e "  ${BOLD}IDs Criados durante os testes:${NC}"
    echo -e "  • Category ID:  ${CATEGORY_ID:-N/A}"
    echo -e "  • Laptop ID:    ${PRODUCT_LAPTOP_ID:-N/A}"
    echo -e "  • Mouse ID:     ${PRODUCT_MOUSE_ID:-N/A}"
    echo -e "  • Order ID:     ${ORDER_ID:-N/A}"
    echo -e "${YELLOW}???????????????????????????????????????????????????????????????????${NC}"
    echo ""
}

# ============================================================================
# MAIN
# ============================================================================

main() {
    clear
    
    echo ""
    echo -e "${CYAN}?????????????????????????????????????????????????????????????????????${NC}"
    echo -e "${CYAN}?                                                                   ?${NC}"
    echo -e "${CYAN}?   ${BOLD}?? SHOP MICROSERVICES - END-TO-END TESTS${NC}${CYAN}                       ?${NC}"
    echo -e "${CYAN}?                                                                   ?${NC}"
    echo -e "${CYAN}?   Catalog API:  $CATALOG_API                            ?${NC}"
    echo -e "${CYAN}?   Basket API:   $BASKET_API                            ?${NC}"
    echo -e "${CYAN}?   Ordering API: $ORDERING_API                            ?${NC}"
    echo -e "${CYAN}?                                                                   ?${NC}"
    echo -e "${CYAN}?????????????????????????????????????????????????????????????????????${NC}"
    echo ""
    
    # Verificar se curl está instalado
    if ! command -v curl &> /dev/null; then
        echo -e "${RED}? ERRO: curl não está instalado!${NC}"
        exit 1
    fi
    
    # Verificar se bc está instalado (para delay)
    if ! command -v bc &> /dev/null; then
        echo -e "${YELLOW}??  bc não encontrado, usando sleep 1 como fallback${NC}"
        DELAY_MS=1000
    fi
    
    # Executar cenários
    check_services_health
    scenario_1_complete_purchase_flow
    scenario_2_validations
    scenario_3_cleanup
    print_summary
    
    # Exit code baseado em falhas
    if [ $TESTS_FAILED -gt 0 ]; then
        exit 1
    fi
    exit 0
}

# Executar main
main "$@"
