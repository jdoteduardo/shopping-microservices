# ============================================================================
# Shop Microservices - End-to-End Tests (PowerShell)
# ============================================================================
# Script de testes end-to-end para validar os microserviços
#
# USO:
#   .\tests\e2e-tests.ps1
#
# PRÉ-REQUISITOS:
#   - Docker Compose rodando com todos os serviços healthy
#   - PowerShell 5.1+ ou PowerShell Core
#
# PORTAS:
#   - Catalog API: 5001
#   - Basket API:  5002
#   - Ordering API: 5003
# ============================================================================

param(
    [int]$DelayMs = 500
)

# ============================================================================
# CONFIGURAÇÃO
# ============================================================================

$CATALOG_API = "http://localhost:5001"
$BASKET_API = "http://localhost:5002"
$ORDERING_API = "http://localhost:5003"

# Gerar sufixo único para evitar conflitos de duplicidade
$script:TestSuffix = Get-Date -Format "yyyyMMddHHmmss"

# Contadores
$script:TestsPassed = 0
$script:TestsFailed = 0
$script:TestsTotal = 0

# IDs criados
$script:CategoryId = $null
$script:ProductLaptopId = $null
$script:ProductMouseId = $null
$script:OrderId = $null

# ============================================================================
# FUNÇÕES AUXILIARES
# ============================================================================

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Blue
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Blue
    Write-Host ""
}

function Write-SubHeader {
    param([string]$Text)
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Yellow
    Write-Host "  $Text" -ForegroundColor White
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Yellow
}

function Write-Test {
    param([string]$Text)
    Write-Host "? TEST: $Text" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "  ? PASS: $Text" -ForegroundColor Green
    $script:TestsPassed++
    $script:TestsTotal++
}

function Write-Fail {
    param([string]$Text)
    Write-Host "  ? FAIL: $Text" -ForegroundColor Red
    $script:TestsFailed++
    $script:TestsTotal++
}

function Write-Info {
    param([string]$Text)
    Write-Host "  ??  INFO: $Text" -ForegroundColor Blue
}

function Write-Warning {
    param([string]$Text)
    Write-Host "  ??  WARN: $Text" -ForegroundColor Yellow
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body,
        [int]$ExpectedStatus,
        [string]$Description
    )
    
    Write-Test $Description
    
    try {
        $params = @{
            Method = $Method
            Uri = $Uri
            ContentType = "application/json"
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params
        $statusCode = $response.StatusCode
        $content = $response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
        
        if ($statusCode -eq $ExpectedStatus) {
            Write-Success "HTTP $statusCode (expected $ExpectedStatus)"
            return @{
                Success = $true
                StatusCode = $statusCode
                Content = $content
                Raw = $response.Content
            }
        } else {
            Write-Fail "HTTP $statusCode (expected $ExpectedStatus)"
            return @{
                Success = $false
                StatusCode = $statusCode
                Content = $content
                Raw = $response.Content
            }
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq $ExpectedStatus) {
            Write-Success "HTTP $statusCode (expected $ExpectedStatus)"
            return @{
                Success = $true
                StatusCode = $statusCode
                Content = $null
                Raw = $_.ErrorDetails.Message
            }
        } else {
            Write-Fail "HTTP $statusCode (expected $ExpectedStatus)"
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
            return @{
                Success = $false
                StatusCode = $statusCode
                Content = $null
                Raw = $_.ErrorDetails.Message
            }
        }
    }
    finally {
        Start-Sleep -Milliseconds $DelayMs
    }
}

# ============================================================================
# VERIFICAÇÃO DE SAÚDE
# ============================================================================

function Test-ServicesHealth {
    Write-Header "?? VERIFICAÇÃO DE SAÚDE DOS SERVIÇOS"
    
    $allHealthy = $true
    
    # Catalog API
    Write-Test "Catalog API Health Check"
    try {
        $response = Invoke-RestMethod -Uri "$CATALOG_API/health" -Method Get -ErrorAction Stop
        Write-Success "Catalog API está healthy"
    }
    catch {
        Write-Fail "Catalog API não está respondendo"
        $allHealthy = $false
    }
    Start-Sleep -Milliseconds $DelayMs
    
    # Basket API
    Write-Test "Basket API Health Check"
    try {
        $response = Invoke-RestMethod -Uri "$BASKET_API/health" -Method Get -ErrorAction Stop
        Write-Success "Basket API está healthy"
    }
    catch {
        Write-Fail "Basket API não está respondendo"
        $allHealthy = $false
    }
    Start-Sleep -Milliseconds $DelayMs
    
    # Ordering API
    Write-Test "Ordering API Health Check"
    try {
        $response = Invoke-RestMethod -Uri "$ORDERING_API/health" -Method Get -ErrorAction Stop
        Write-Success "Ordering API está healthy"
    }
    catch {
        Write-Fail "Ordering API não está respondendo"
        $allHealthy = $false
    }
    Start-Sleep -Milliseconds $DelayMs
    
    if (-not $allHealthy) {
        Write-Host ""
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Red
        Write-Host "  ? ERRO: Nem todos os serviços estão healthy!" -ForegroundColor Red
        Write-Host "  Execute: docker-compose ps" -ForegroundColor Red
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# CENÁRIO 1: FLUXO COMPLETO
# ============================================================================

function Test-Scenario1-CompletePurchaseFlow {
    Write-Header "?? CENÁRIO 1: FLUXO COMPLETO DE COMPRA"
    
    # 1.1 Criar categoria com nome único
    Write-SubHeader "1.1 Criar categoria 'Electronics-$($script:TestSuffix)'"
    $body = @{
        name = "Electronics-$($script:TestSuffix)"
        description = "Electronic devices and gadgets - Test $($script:TestSuffix)"
    }
    $result = Invoke-ApiRequest -Method "POST" -Uri "$CATALOG_API/api/categories" -Body $body -ExpectedStatus 201 -Description "Criar categoria Electronics-$($script:TestSuffix)"
    if ($result.Success -and $result.Content) {
        $script:CategoryId = $result.Content.id
        Write-Info "Category ID: $($script:CategoryId)"
    } else {
        Write-Warning "Falha ao criar categoria. Tentando buscar categoria existente..."
        # Fallback: tentar obter categoria existente pelo ID 1 (seed data)
        try {
            $existingCat = Invoke-RestMethod -Uri "$CATALOG_API/api/categories/1" -Method Get -ErrorAction SilentlyContinue
            if ($existingCat) {
                $script:CategoryId = $existingCat.id
                Write-Info "Usando categoria existente ID: $($script:CategoryId)"
            }
        } catch {
            $script:CategoryId = 1  # Fallback para seed data
            Write-Info "Usando categoria padrão ID: 1"
        }
    }
    
    # 1.2 Criar Laptop com nome único
    Write-SubHeader "1.2 Criar produto 'Laptop-$($script:TestSuffix)'"
    $body = @{
        name = "Laptop Gamer $($script:TestSuffix)"
        description = "High-performance gaming laptop"
        price = 1999.99
        stock = 50
        categoryId = $script:CategoryId
        createdBy = "e2e-test"
    }
    $result = Invoke-ApiRequest -Method "POST" -Uri "$CATALOG_API/api/products" -Body $body -ExpectedStatus 201 -Description "Criar produto Laptop"
    if ($result.Success -and $result.Content) {
        $script:ProductLaptopId = $result.Content.id
        Write-Info "Product Laptop ID: $($script:ProductLaptopId)"
    }
    
    # Verificar se o produto foi criado
    if (-not $script:ProductLaptopId -or $script:ProductLaptopId -eq 0) {
        Write-Warning "Produto Laptop não foi criado. Usando produto existente ID: 1"
        $script:ProductLaptopId = 1
    }
    
    # 1.3 Criar Mouse com nome único
    Write-SubHeader "1.3 Criar produto 'Mouse-$($script:TestSuffix)'"
    $body = @{
        name = "Gaming Mouse $($script:TestSuffix)"
        description = "RGB gaming mouse with 16000 DPI"
        price = 79.99
        stock = 100
        categoryId = $script:CategoryId
        createdBy = "e2e-test"
    }
    $result = Invoke-ApiRequest -Method "POST" -Uri "$CATALOG_API/api/products" -Body $body -ExpectedStatus 201 -Description "Criar produto Mouse"
    if ($result.Success -and $result.Content) {
        $script:ProductMouseId = $result.Content.id
        Write-Info "Product Mouse ID: $($script:ProductMouseId)"
    }
    
    # Verificar se o produto foi criado
    if (-not $script:ProductMouseId -or $script:ProductMouseId -eq 0) {
        Write-Warning "Produto Mouse não foi criado. Usando produto existente ID: 2"
        $script:ProductMouseId = 2
    }
    
    # 1.4 Listar produtos
    Write-SubHeader "1.4 Listar todos os produtos"
    Invoke-ApiRequest -Method "GET" -Uri "$CATALOG_API/api/products" -ExpectedStatus 200 -Description "Listar produtos"
    
    # 1.5 Adicionar Laptop ao carrinho
    Write-SubHeader "1.5 Adicionar Laptop ao carrinho do user123-$($script:TestSuffix)"
    $body = @{
        productId = $script:ProductLaptopId
        productName = "Laptop Gamer $($script:TestSuffix)"
        price = 1999.99
        quantity = 1
    }
    Invoke-ApiRequest -Method "POST" -Uri "$BASKET_API/api/basket/user123-$($script:TestSuffix)/items" -Body $body -ExpectedStatus 200 -Description "Adicionar Laptop ao carrinho"
    
    # 1.6 Adicionar Mouse ao carrinho
    Write-SubHeader "1.6 Adicionar Mouse ao carrinho do user123-$($script:TestSuffix)"
    $body = @{
        productId = $script:ProductMouseId
        productName = "Gaming Mouse $($script:TestSuffix)"
        price = 79.99
        quantity = 2
    }
    Invoke-ApiRequest -Method "POST" -Uri "$BASKET_API/api/basket/user123-$($script:TestSuffix)/items" -Body $body -ExpectedStatus 200 -Description "Adicionar Mouse ao carrinho"
    
    # 1.7 Obter carrinho
    Write-SubHeader "1.7 Obter carrinho do user123-$($script:TestSuffix)"
    $result = Invoke-ApiRequest -Method "GET" -Uri "$BASKET_API/api/basket/user123-$($script:TestSuffix)" -ExpectedStatus 200 -Description "Obter carrinho"
    if ($result.Success -and $result.Content) {
        Write-Info "Total: $($result.Content.totalPrice)"
    }
    
    # 1.8 Criar pedido
    Write-SubHeader "1.8 Criar pedido"
    $body = @{
        userId = "user123-$($script:TestSuffix)"
        items = @(
            @{
                productId = [int]$script:ProductLaptopId
                productName = "Laptop Gamer $($script:TestSuffix)"
                price = 1999.99
                quantity = 1
            },
            @{
                productId = [int]$script:ProductMouseId
                productName = "Gaming Mouse $($script:TestSuffix)"
                price = 79.99
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
    }
    
    Write-Info "Enviando pedido com ProductLaptopId=$($script:ProductLaptopId), ProductMouseId=$($script:ProductMouseId)"
    
    $result = Invoke-ApiRequest -Method "POST" -Uri "$ORDERING_API/api/orders" -Body $body -ExpectedStatus 201 -Description "Criar pedido"
    if ($result.Success -and $result.Content) {
        $script:OrderId = $result.Content.id
        Write-Info "Order ID: $($script:OrderId)"
    } else {
        Write-Warning "Falha ao criar pedido. Response: $($result.Raw)"
    }
    
    # 1.9 Verificar pedido
    Write-SubHeader "1.9 Verificar pedido criado"
    if ($script:OrderId) {
        Invoke-ApiRequest -Method "GET" -Uri "$ORDERING_API/api/orders/$($script:OrderId)" -ExpectedStatus 200 -Description "Obter pedido por ID"
    } else {
        Write-Warning "Order ID não disponível - pulando teste de verificação"
    }
    
    # 1.10 Atualizar status
    Write-SubHeader "1.10 Atualizar status para 'Confirmed'"
    if ($script:OrderId) {
        $body = @{ status = 1 }
        Invoke-ApiRequest -Method "PUT" -Uri "$ORDERING_API/api/orders/$($script:OrderId)/status" -Body $body -ExpectedStatus 200 -Description "Atualizar status para Confirmed"
    } else {
        Write-Warning "Order ID não disponível - pulando teste de atualização de status"
    }
}

# ============================================================================
# CENÁRIO 2: VALIDAÇÕES
# ============================================================================

function Test-Scenario2-Validations {
    Write-Header "?? CENÁRIO 2: VALIDAÇÕES"
    
    # 2.1 Produto inválido
    Write-SubHeader "2.1 Criar produto com dados inválidos"
    $body = @{
        name = ""
        description = "Test"
        price = -10
        stock = -5
        categoryId = 0
        createdBy = ""
    }
    Invoke-ApiRequest -Method "POST" -Uri "$CATALOG_API/api/products" -Body $body -ExpectedStatus 400 -Description "Criar produto com dados inválidos (deve falhar)"
    
    # 2.2 Produto sem nome
    Write-SubHeader "2.2 Criar produto sem nome"
    $body = @{
        name = ""
        description = "Test product"
        price = 99.99
        stock = 10
        categoryId = $script:CategoryId
        createdBy = "test"
    }
    Invoke-ApiRequest -Method "POST" -Uri "$CATALOG_API/api/products" -Body $body -ExpectedStatus 400 -Description "Criar produto sem nome (deve falhar)"
    
    # 2.3 Item com quantidade negativa
    Write-SubHeader "2.3 Adicionar item com quantidade negativa"
    $body = @{
        productId = 1
        productName = "Test"
        price = 10.00
        quantity = -5
    }
    Invoke-ApiRequest -Method "POST" -Uri "$BASKET_API/api/basket/usertest/items" -Body $body -ExpectedStatus 400 -Description "Adicionar item com quantidade negativa (deve falhar)"
    
    # 2.4 Pedido vazio
    Write-SubHeader "2.4 Criar pedido sem itens"
    $body = @{
        userId = "usertest"
        items = @()
        shippingAddress = @{
            street = "Test Street"
            city = "Test City"
            state = "TS"
            zipCode = "12345"
            country = "Test"
        }
    }
    Invoke-ApiRequest -Method "POST" -Uri "$ORDERING_API/api/orders" -Body $body -ExpectedStatus 400 -Description "Criar pedido sem itens (deve falhar)"
    
    # 2.5 Pedido sem userId
    Write-SubHeader "2.5 Criar pedido sem userId"
    $body = @{
        userId = ""
        items = @(
            @{
                productId = 1
                productName = "Test"
                price = 10.00
                quantity = 1
            }
        )
        shippingAddress = @{
            street = "Test Street"
            city = "Test City"
            state = "TS"
            zipCode = "12345"
            country = "Test"
        }
    }
    Invoke-ApiRequest -Method "POST" -Uri "$ORDERING_API/api/orders" -Body $body -ExpectedStatus 400 -Description "Criar pedido sem userId (deve falhar)"
    
    # 2.6 Pedido inexistente
    Write-SubHeader "2.6 Obter pedido inexistente"
    Invoke-ApiRequest -Method "GET" -Uri "$ORDERING_API/api/orders/000000000000000000000000" -ExpectedStatus 404 -Description "Obter pedido inexistente (deve retornar 404)"
}

# ============================================================================
# CENÁRIO 3: LIMPEZA
# ============================================================================

function Test-Scenario3-Cleanup {
    Write-Header "?? CENÁRIO 3: LIMPEZA"
    
    # 3.1 Limpar carrinho
    Write-SubHeader "3.1 Limpar carrinho do user123-$($script:TestSuffix)"
    Write-Test "Limpar carrinho"
    try {
        Invoke-RestMethod -Uri "$BASKET_API/api/basket/user123-$($script:TestSuffix)" -Method Delete -ErrorAction Stop
        Write-Success "Carrinho limpo com sucesso"
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 404) {
            Write-Success "Carrinho já estava vazio (HTTP 404)"
        } else {
            Write-Fail "Falha ao limpar carrinho (HTTP $statusCode)"
        }
    }
    $script:TestsTotal++
    Start-Sleep -Milliseconds $DelayMs
    
    # 3.2 Verificar carrinho vazio
    Write-SubHeader "3.2 Verificar carrinho vazio"
    Invoke-ApiRequest -Method "GET" -Uri "$BASKET_API/api/basket/user123-$($script:TestSuffix)" -ExpectedStatus 200 -Description "Verificar carrinho vazio"
    
    # 3.3 Criar pedido para cancelamento
    Write-SubHeader "3.3 Criar pedido para teste de cancelamento"
    $body = @{
        userId = "user-cleanup-$($script:TestSuffix)"
        items = @(
            @{
                productId = 1
                productName = "Test Product for Cancel"
                price = 10.00
                quantity = 1
            }
        )
        shippingAddress = @{
            street = "Test Street"
            city = "Test City"
            state = "TS"
            zipCode = "12345"
            country = "Test"
        }
    }
    $result = Invoke-ApiRequest -Method "POST" -Uri "$ORDERING_API/api/orders" -Body $body -ExpectedStatus 201 -Description "Criar pedido para cancelamento"
    $cancelOrderId = $null
    if ($result.Success -and $result.Content) {
        $cancelOrderId = $result.Content.id
        Write-Info "Cancel Order ID: $cancelOrderId"
    }
    
    # 3.4 Cancelar pedido
    Write-SubHeader "3.4 Cancelar pedido"
    if ($cancelOrderId) {
        Write-Test "Cancelar pedido"
        try {
            Invoke-RestMethod -Uri "$ORDERING_API/api/orders/$cancelOrderId" -Method Delete -ErrorAction Stop
            Write-Success "Pedido cancelado com sucesso"
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            if ($statusCode -eq 204) {
                Write-Success "Pedido cancelado com sucesso"
            } else {
                Write-Fail "Falha ao cancelar pedido (HTTP $statusCode)"
            }
        }
        $script:TestsTotal++
        Start-Sleep -Milliseconds $DelayMs
    } else {
        Write-Warning "Não foi possível criar pedido para cancelamento - pulando teste"
    }
    
    # 3.5 Verificar status
    Write-SubHeader "3.5 Verificar status do pedido cancelado"
    if ($cancelOrderId) {
        Invoke-ApiRequest -Method "GET" -Uri "$ORDERING_API/api/orders/$cancelOrderId" -ExpectedStatus 200 -Description "Verificar status Cancelled"
    } else {
        Write-Warning "Order ID não disponível - pulando verificação de status"
    }
}

# ============================================================================
# SUMMARY
# ============================================================================

function Show-Summary {
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Blue
    Write-Host "  ?? RESUMO DOS TESTES" -ForegroundColor Cyan
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Blue
    Write-Host ""
    Write-Host "  ? Testes Passaram:  $($script:TestsPassed)" -ForegroundColor Green
    Write-Host "  ? Testes Falharam:  $($script:TestsFailed)" -ForegroundColor Red
    Write-Host "  ?? Total de Testes:  $($script:TestsTotal)" -ForegroundColor Blue
    Write-Host ""
    
    if ($script:TestsFailed -eq 0) {
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "  ?? TODOS OS TESTES PASSARAM!" -ForegroundColor Green
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Green
    } else {
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Red
        Write-Host "  ??  ALGUNS TESTES FALHARAM!" -ForegroundColor Red
        Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Yellow
    Write-Host "  IDs Criados durante os testes:" -ForegroundColor White
    Write-Host "  • Category ID:  $(if ($script:CategoryId) { $script:CategoryId } else { 'N/A' })"
    Write-Host "  • Laptop ID:    $(if ($script:ProductLaptopId) { $script:ProductLaptopId } else { 'N/A' })"
    Write-Host "  • Mouse ID:     $(if ($script:ProductMouseId) { $script:ProductMouseId } else { 'N/A' })"
    Write-Host "  • Order ID:     $(if ($script:OrderId) { $script:OrderId } else { 'N/A' })"
    Write-Host "???????????????????????????????????????????????????????????????????" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# MAIN
# ============================================================================

Clear-Host

Write-Host ""
Write-Host "?????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                                                                   ?" -ForegroundColor Cyan
Write-Host "?   ?? SHOP MICROSERVICES - END-TO-END TESTS                       ?" -ForegroundColor Cyan
Write-Host "?                                                                   ?" -ForegroundColor Cyan
Write-Host "?   Catalog API:  $CATALOG_API                            ?" -ForegroundColor Cyan
Write-Host "?   Basket API:   $BASKET_API                            ?" -ForegroundColor Cyan
Write-Host "?   Ordering API: $ORDERING_API                            ?" -ForegroundColor Cyan
Write-Host "?                                                                   ?" -ForegroundColor Cyan
Write-Host "?????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Executar cenários
Test-ServicesHealth
Test-Scenario1-CompletePurchaseFlow
Test-Scenario2-Validations
Test-Scenario3-Cleanup
Show-Summary

# Exit code
if ($script:TestsFailed -gt 0) {
    exit 1
}
exit 0
