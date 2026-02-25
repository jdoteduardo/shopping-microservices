#!/bin/bash

# Configuration
BASE_URL="http://localhost:5000"
USER_EMAIL="user@example.com"
USER_PASSWORD="Test@123"
ADMIN_EMAIL="admin@example.com"
ADMIN_PASSWORD="Admin@123"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}===================================================${NC}"
echo -e "${BLUE}     SHOP MICROSERVICES MANUAL TEST SCRIPT         ${NC}"
echo -e "${BLUE}===================================================${NC}"

# Helper function for extraction without jq
extract_json_value() {
    local key=$1
    local json=$2
    echo "$json" | sed -n "s/.*\"$key\":\"\([^\"]*\)\".*/\1/p"
}

# 0. Login as Regular User
echo -e "\n${YELLOW}0. Logging in as regular user ($USER_EMAIL)...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$USER_EMAIL\",\"password\":\"$USER_PASSWORD\"}")

TOKEN=$(extract_json_value "token" "$LOGIN_RESPONSE")

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    echo -e "${RED}FAILED: Could not obtain token. Response: $LOGIN_RESPONSE${NC}"
    exit 1
fi

echo -e "${GREEN}SUCCESS: Token obtained.${NC}"

# 1. Create Category
echo -e "\n${YELLOW}1. Creating category 'Electronics'...${NC}"
CAT_RESPONSE=$(curl -s -X POST "$BASE_URL/api/catalog/categories" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"name":"Electronics","description":"Gadgets and devices"}')

CAT_ID=$(extract_json_value "id" "$CAT_RESPONSE")

if [ -z "$CAT_ID" ]; then
    if echo "$CAT_RESPONSE" | grep -q "DUPLICATE"; then
        echo -e "${BLUE}INFO: Category 'Electronics' already exists. Using seeded ID 1.${NC}"
        CAT_ID=1
    else
        echo -e "${RED}FAILED to create category. Response: $CAT_RESPONSE${NC}"
    fi
fi
if [ -n "$CAT_ID" ]; then
    echo -e "${GREEN}SUCCESS: Category ID: $CAT_ID${NC}"
fi

# 2. Create Product
echo -e "\n${YELLOW}2. Creating product 'Smartphone'...${NC}"
PROD_RESPONSE=$(curl -s -X POST "$BASE_URL/api/catalog/products" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"name\":\"Smartphone\",\"description\":\"Latest flagship\",\"price\":999.99,\"categoryId\":${CAT_ID:-1}}")

PROD_ID=$(extract_json_value "id" "$PROD_RESPONSE")

if [ -z "$PROD_ID" ]; then
    if echo "$PROD_RESPONSE" | grep -q "DUPLICATE"; then
        echo -e "${BLUE}INFO: Product 'Smartphone' already exists. Using seeded ID 1.${NC}"
        PROD_ID=1
    else
        echo -e "${RED}FAILED to create product. Response: $PROD_RESPONSE${NC}"
    fi
fi
if [ -n "$PROD_ID" ]; then
    echo -e "${GREEN}SUCCESS: Product ID: $PROD_ID${NC}"
fi

# 3. Add to Basket
echo -e "\n${YELLOW}3. Adding to basket...${NC}"
BASKET_RESPONSE=$(curl -s -X POST "$BASE_URL/api/basket/items" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"productId\":${PROD_ID:-1},\"productName\":\"Smartphone\",\"price\":999.99,\"quantity\":1}")

if echo "$BASKET_RESPONSE" | grep -q "items"; then
    echo -e "${GREEN}SUCCESS: Item added to basket.${NC}"
else
    echo -e "${RED}FAILED to add to basket. Response: $BASKET_RESPONSE${NC}"
fi

# 4. Create Order
echo -e "\n${YELLOW}4. Creating order...${NC}"
ORDER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/ordering/orders" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
        \"items\": [
            {\"productId\": ${PROD_ID:-1}, \"productName\": \"Smartphone\", \"price\": 999.99, \"quantity\": 1}
        ],
        \"shippingAddress\": {
            \"street\": \"123 Main St\",
            \"city\": \"TechCity\",
            \"state\": \"TS\",
            \"zipCode\": \"12345\",
            \"country\": \"USA\"
        }
    }")

ORDER_ID=$(extract_json_value "id" "$ORDER_RESPONSE")

if [ -z "$ORDER_ID" ]; then
    echo -e "${RED}FAILED to create order. Response: $ORDER_RESPONSE${NC}"
else
    echo -e "${GREEN}SUCCESS: Order created with ID: $ORDER_ID${NC}"
fi

# 5. Get My Orders
echo -e "\n${YELLOW}5. Getting personal orders...${NC}"
MY_ORDERS=$(curl -s -X GET "$BASE_URL/api/ordering/orders/my" \
    -H "Authorization: Bearer $TOKEN")

if echo "$MY_ORDERS" | grep -q "orderNumber"; then
    echo -e "${GREEN}SUCCESS: Retrieved personal orders.${NC}"
else
    echo -e "${RED}FAILED to retrieve orders. Response: $MY_ORDERS${NC}"
fi

# 6. Test 401 Unauthorized
echo -e "\n${YELLOW}6. Testing access WITHOUT TOKEN (Expected 401)...${NC}"
STATUS_401=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/auth/me")

if [ "$STATUS_401" == "401" ]; then
    echo -e "${GREEN}SUCCESS: Correctly returned 401 Unauthorized.${NC}"
else
    echo -e "${RED}FAILED: Expected 401 but got $STATUS_401${NC}"
fi

# 7. Test 403 Forbidden
echo -e "\n${YELLOW}7. Testing Admin endpoint with regular User token (Expected 403)...${NC}"
STATUS_403=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/ordering/orders/all" \
    -H "Authorization: Bearer $TOKEN")

if [ "$STATUS_403" == "403" ]; then
    echo -e "${GREEN}SUCCESS: Correctly returned 403 Forbidden.${NC}"
else
    # In some microservices setups, if auth isn't fully propagated, it might return 401 or 400.
    # We expect 403 if the Gateway/Service correctly identifies the lack of Admin role.
    echo -e "${RED}FAILED: Expected 403 but got $STATUS_403${NC}"
fi

# 8. Admin Access
echo -e "\n${YELLOW}8. Logging in as ADMIN and testing Admin access...${NC}"
ADMIN_LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}")

ADMIN_TOKEN=$(extract_json_value "token" "$ADMIN_LOGIN_RESPONSE")

if [ -n "$ADMIN_TOKEN" ] && [ "$ADMIN_TOKEN" != "null" ]; then
    STATUS_ADMIN=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/ordering/orders/all" \
        -H "Authorization: Bearer $ADMIN_TOKEN")
    
    if [ "$STATUS_ADMIN" == "200" ]; then
        echo -e "${GREEN}SUCCESS: Admin access granted (200 OK).${NC}"
    else
        echo -e "${RED}FAILED: Admin access denied with status $STATUS_ADMIN${NC}"
    fi
else
    echo -e "${RED}FAILED: Could not obtain Admin token.${NC}"
fi

echo -e "\n${BLUE}===================================================${NC}"
echo -e "${BLUE}            MANUAL TESTS COMPLETED                 ${NC}"
echo -e "${BLUE}===================================================${NC}"
