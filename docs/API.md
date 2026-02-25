# API Documentation

## 1. Overview

Esta documentação descreve as APIs do sistema de microservices de e-commerce.

### Base URLs (Local Development)

| API | Base URL | Porta |
|-----|----------|-------|
| Catalog API | `http://localhost:5001` | 5001 |
| Basket API | `http://localhost:5002` | 5002 |
| Ordering API | `http://localhost:5003` | 5003 |

### Formato de Responses
Todas as APIs utilizam JSON como formato padrão para troca de dados.
Header `Content-Type: application/json` deve ser utilizado em requests `POST` e `PUT`.

### Status Codes Comuns

- `200 OK`: Requisição processada com sucesso.
- `201 Created`: Recurso criado com sucesso.
- `204 No Content`: Requisição processada com sucesso, sem conteúdo de retorno.
- `400 Bad Request`: Erro na requisição (validação ou formato inválido).
- `404 Not Found`: Recurso não encontrado.
- `500 Internal Server Error`: Erro inesperado no servidor.

---

## 2. Catalog API
**Base URL**: `http://localhost:5001`

### Get Products
Retorna a lista de produtos cadastrados.

- **Método**: `GET`
- **Path**: `/api/products`
- **Status Codes**: `200`

**Response Example:**
```json
[
  {
    "id": 1,
    "name": "Smartphone XYZ",
    "description": "Latest model smartphone",
    "price": 999.99,
    "stock": 50,
    "categoryId": 1,
    "categoryName": "Electronics",
    "createdBy": "admin",
    "createdAt": "2023-10-27T10:00:00Z",
    "updatedAt": null
  },
  {
    "id": 2,
    "name": "Laptop Pro",
    "description": "High performance laptop",
    "price": 1499.99,
    "stock": 20,
    "categoryId": 1,
    "categoryName": "Electronics",
    "createdBy": "admin",
    "createdAt": "2023-10-27T10:05:00Z",
    "updatedAt": null
  }
]
```

### Create Product
Cria um novo produto.

- **Método**: `POST`
- **Path**: `/api/products`
- **Status Codes**: `201`, `400`

**Request Body:**
```json
{
  "name": "New Product",
  "description": "Product description",
  "price": 49.99,
  "stock": 100,
  "categoryId": 1,
  "createdBy": "admin"
}
```

**Response Example:**
```json
{
  "id": 3,
  "name": "New Product",
  "description": "Product description",
  "price": 49.99,
  "stock": 100,
  "categoryId": 1,
  "categoryName": "Electronics",
  "createdBy": "admin",
  "createdAt": "2023-10-27T10:10:00Z",
  "updatedAt": null
}
```

### Get Categories
Retorna a lista de categorias.

- **Método**: `GET`
- **Path**: `/api/categories`
- **Status Codes**: `200`

**Response Example:**
```json
[
  {
    "id": 1,
    "name": "Electronics",
    "description": "Gadgets and devices"
  },
  {
    "id": 2,
    "name": "Books",
    "description": "Printed and digital books"
  }
]
```

---

## 3. Basket API
**Base URL**: `http://localhost:5002`

### Get Basket
Retorna o carrinho de compras de um usuário.

- **Método**: `GET`
- **Path**: `/api/basket/{userId}`
- **Status Codes**: `200`

**Response Example:**
```json
{
  "userId": "user123",
  "items": [
    {
      "productId": 1,
      "productName": "Smartphone XYZ",
      "price": 999.99,
      "quantity": 1,
      "subtotal": 999.99
    }
  ],
  "totalPrice": 999.99
}
```

### Update Basket
Atualiza ou cria o carrinho de compras do usuário.

- **Método**: `POST`
- **Path**: `/api/basket`
- **Status Codes**: `200`, `400`

**Request Body:**
```json
{
  "userId": "user123",
  "items": [
    {
      "productId": 1,
      "productName": "Smartphone XYZ",
      "price": 999.99,
      "quantity": 2
    }
  ]
}
```

**Response Example:**
```json
{
  "userId": "user123",
  "items": [
    {
      "productId": 1,
      "productName": "Smartphone XYZ",
      "price": 999.99,
      "quantity": 2,
      "subtotal": 1999.98
    }
  ],
  "totalPrice": 1999.98
}
```

### Delete Basket
Remove o carrinho de compras do usuário.

- **Método**: `DELETE`
- **Path**: `/api/basket/{userId}`
- **Status Codes**: `204`, `404`

---

## 4. Ordering API
**Base URL**: `http://localhost:5003`

### Get Orders
Retorna todos os pedidos (necessita permissão administrativa ou retorna lista vazia).

- **Método**: `GET`
- **Path**: `/api/orders`
- **Status Codes**: `200`

**Response Example:**
```json
[
  {
    "id": "order-guid-1",
    "orderNumber": "ORD-2023-001",
    "userId": "user123",
    "orderDate": "2023-10-27T10:15:00Z",
    "status": 1,
    "statusDescription": "Pending",
    "totalAmount": 1999.98,
    "items": [
      {
        "productId": 1,
        "productName": "Smartphone XYZ",
        "price": 999.99,
        "quantity": 2
      }
    ],
    "shippingAddress": {
      "street": "123 Main St",
      "city": "Metropolis",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    }
  }
]
```

### Create Order
Cria um novo pedido a partir dos itens do carrinho.

- **Método**: `POST`
- **Path**: `/api/orders`
- **Status Codes**: `201`, `400`

**Request Body:**
```json
{
  "userId": "user123",
  "items": [
    {
      "productId": 1,
      "productName": "Smartphone XYZ",
      "price": 999.99,
      "quantity": 2
    }
  ],
  "shippingAddress": {
    "street": "123 Main St",
    "city": "Metropolis",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  }
}
```

### Get Order by ID
Retorna os detalhes de um pedido específico.

- **Método**: `GET`
- **Path**: `/api/orders/{id}`
- **Status Codes**: `200`, `404`

---

## 5. Error Responses

Em caso de erros (400, 404, 500), a API retorna um objeto JSON contendo a mensagem de erro.

**Formato Padrão:**
```json
{
  "message": "Descrição detalhada do erro ocorrido."
}
```

**Exemplo - 404 Not Found:**
```json
{
  "message": "Order with id order-guid-999 not found"
}
```

**Exemplo - 400 Bad Request:**
```json
{
  "message": "Quantity cannot be negative"
}
```
