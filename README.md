# NaturalShop E-Commerce Backend API

A modern, production-ready .NET 8 e-commerce backend solution featuring JWT authentication, order processing, payment integration, and comprehensive product management for a natural products marketplace.

**Repository:** [48pinaribs/NaturalShop.API](https://github.com/48pinaribs/NaturalShop.API)

---

## 📋 Table of Contents

1. [Architectural Overview](#architectural-overview)
2. [Core Business Logic](#core-business-logic)
3. [Security Framework](#security-framework)
4. [Database Schema](#database-schema)
5. [API Patterns & Best Practices](#api-patterns--best-practices)
6. [Technology Stack](#technology-stack)
7. [API Endpoints Documentation](#api-endpoints-documentation)
8. [Database Setup & Migrations](#database-setup--migrations)
9. [Development Setup](#development-setup)
10. [Deployment Guide](#deployment-guide)

---

## 🏗️ Architectural Overview

### System Design Philosophy

The NaturalShop API follows a **layered architecture pattern** designed for scalability, maintainability, and separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│         API Controllers / REST Endpoints                │
├─────────────────────────────────────────────────────────┤
│  Authorization & Authentication (JWT Bearer Tokens)     │
├─────────────────────────────────────────────────────────┤
│  Business Logic Layer                                    │
│  ├─ Order Processing Service                           │
│  ├─ Payment Integration (Iyzipay)                      │
│  ├─ SMS Verification Service                           │
│  └─ Product Management                                 │
├─────────────────────────────────────────────────────────┤
│  Data Transfer Objects (DTOs)                           │
│  + AutoMapper Configuration                            │
├─────────────────────────────────────────────────────────┤
│  Entity Framework Core + PostgreSQL                     │
│  ├─ Entity Models                                       │
│  ├─ DbContext Configuration                            │
│  └─ Migrations                                         │
├─────────────────────────────────────────────────────────┤
│  External Integrations                                  │
│  ├─ Iyzipay (Payment Gateway)                          │
│  ├─ Netgsm (SMS Service)                               │
│  └─ Azure/Ngrok (Webhooks)                             │
└─────────────────────────────────────────────────────────┘
```

### High-Level Features

| Feature                  | Implementation                             | Status         |
| ------------------------ | ------------------------------------------ | -------------- |
| **User Authentication**  | JWT Bearer Tokens (7-day expiration)       | ✅ Implemented |
| **User Authorization**   | Role-based access control (Identity roles) | ✅ Implemented |
| **Product Catalog**      | Full CRUD operations with categories       | ✅ Implemented |
| **Shopping Cart**        | Order items with quantity management       | ✅ Implemented |
| **Order Processing**     | Complete order lifecycle management        | ✅ Implemented |
| **Payment Processing**   | Iyzipay integration for secure payments    | ✅ Implemented |
| **Email Verification**   | SMS-based phone verification               | ✅ Implemented |
| **Inventory Management** | Stock tracking with validation             | ✅ Implemented |
| **Image Gallery**        | Multi-image product stories                | ✅ Implemented |
| **CORS Support**         | Multi-domain frontend integration          | ✅ Implemented |

---

## 💼 Core Business Logic

### 1. **Product Catalog Management**

The product system manages a curated collection of natural food products with rich content.

**Product Lifecycle:**

```
[Product Created] → [Stock Tracked] → [Added to Orders] → [Stock Deducted] → [Inventory Updated]
```

**Key Features:**

- Multi-image gallery support (story images for product narratives)
- Category-based organization
- SKU/Code tracking for inventory systems
- Dynamic pricing with decimal precision
- Stock validation before orders

**Product Model:**

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }                    // Product title
    public string? Description { get; set; }            // Short description
    public decimal Price { get; set; }                  // Product price (TRY)
    public string? ImageUrl { get; set; }               // Primary image
    public string? Category { get; set; }               // Product category
    public int Stock { get; set; }                      // Available quantity
    public string Code { get; set; }                    // SKU/Product code
    public string? StoryText { get; set; }              // Product story narrative
    public string[]? StoryImages { get; set; }          // Gallery images for story
    public string[]? Images { get; set; }               // Additional images
    public ICollection<OrderItem> OrderItems { get; set; }  // Order references
}
```

**Supported Categories:**

- Zeytinyağı Grubu (Olive Oil)
- Pekmez & Bal (Molasses & Honey)
- Baharatlar (Spices)
- Kuru Meyveler (Dried Fruits)
- And more...

---

### 2. **Order Processing System**

Complete order lifecycle with secure server-side price validation and inventory management.

**Order Flow Diagram:**

```
┌──────────────────────────────────────────────────────────────┐
│ 1. Customer Creates Order with Items                         │
│    (ProductId, Quantity, User Context)                      │
├──────────────────────────────────────────────────────────────┤
│ 2. Server-Side Validation                                    │
│    ├─ Verify user identity from JWT token                   │
│    ├─ Check product existence in catalog                    │
│    ├─ Validate stock availability                           │
│    └─ Recalculate total from database (security!)           │
├──────────────────────────────────────────────────────────────┤
│ 3. Order Item Creation                                       │
│    └─ Create OrderItem records with unit prices             │
├──────────────────────────────────────────────────────────────┤
│ 4. Payment Initiation                                        │
│    ├─ Generate Iyzipay checkout token                       │
│    └─ Return payment form data to frontend                  │
├──────────────────────────────────────────────────────────────┤
│ 5. Payment Processing (Callback)                            │
│    ├─ Verify payment status from Iyzipay                    │
│    ├─ Deduct stock from inventory                           │
│    └─ Update order status                                   │
├──────────────────────────────────────────────────────────────┤
│ 6. Order Confirmation                                        │
│    └─ Return order details to customer                      │
└──────────────────────────────────────────────────────────────┘
```

**Order Model:**

```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }             // Order timestamp
    public string Status { get; set; }                  // Status: "Hazırlanıyor", "Ödendi", etc.
    public List<OrderItem> Items { get; set; }          // Line items
    public string UserId { get; set; }                  // FK to ApplicationUser
    public decimal TotalAmount { get; set; }            // Total price (TRY)
    public string? IyzipayToken { get; set; }           // Payment gateway token
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }                    // FK to Order
    public int ProductId { get; set; }                  // FK to Product
    public int Quantity { get; set; }                   // Unit quantity
    public string ProductName { get; set; }             // Denormalized for history
    public decimal UnitPrice { get; set; }              // Price snapshot at order time
}
```

**Order Statuses:**

- `Hazırlanıyor` (Preparing)
- `Ödeme Bekleniyor` (Payment Pending)
- `Ödendi` (Paid)
- `Gönderildi` (Shipped)
- `Teslim Edildi` (Delivered)
- `İptal Edildi` (Cancelled)

---

### 3. **Inventory Management**

Server-side validation ensures data integrity and prevents overselling.

**Key Validations:**

```
FOR EACH ITEM IN ORDER:
  1. Quantity > 0 ✓
  2. Product exists in database ✓
  3. Product stock >= requested quantity ✓
  4. Price fetched from database (not client) ✓
  5. Total calculated server-side ✓
```

**Stock Deduction Process:**

```sql
-- Atomic update to prevent race conditions
UPDATE Products
SET Stock = Stock - {quantity}
WHERE Id = {productId}
AND Stock >= {quantity}
```

---

## 🔐 Security Framework

### Authentication & Authorization Strategy

#### 1. **JWT (JSON Web Token) Implementation**

The API uses JWT Bearer tokens for stateless authentication:

```
┌─────────────────────────────────────────────────────────┐
│        REQUEST WITH JWT TOKEN                          │
├─────────────────────────────────────────────────────────┤
│ Header: Authorization: Bearer eyJhbGc...              │
├─────────────────────────────────────────────────────────┤
│ Token Payload (Claims):                                │
│  ├─ NameIdentifier: User ID (GUID)                    │
│  ├─ Email: User email address                         │
│  ├─ iat: Issued at timestamp                          │
│  └─ exp: Expiration (7 days from issue)               │
├─────────────────────────────────────────────────────────┤
│ Validation:                                             │
│  ├─ Signature verification (HMAC-SHA256)              │
│  ├─ Issuer check: "NaturalShop"                       │
│  ├─ Audience check: "NaturalShopUsers"                │
│  └─ Expiration verification                          │
└─────────────────────────────────────────────────────────┘
```

**JWT Configuration:**

```json
{
  "JwtSettings": {
    "Key": "super-secret-key-naturalshop-12345",
    "Issuer": "NaturalShop",
    "Audience": "NaturalShopUsers"
  }
}
```

**Token Claims:**
| Claim | Type | Purpose |
|-------|------|---------|
| `sub` (NameIdentifier) | GUID | Unique user identifier |
| `email` | String | User's email address |
| `iat` | Unix timestamp | Token issued time |
| `exp` | Unix timestamp | Token expiration (7 days) |
| `iss` | String | Token issuer identifier |
| `aud` | String | Token audience identifier |

#### 2. **Password Security**

User passwords are managed through **ASP.NET Core Identity** with PBKDF2 hashing:

```csharp
// Password hashing requirements
PasswordHasher<ApplicationUser> hasher = new();
string hashedPassword = hasher.HashPassword(user, password);

// Hash includes:
// - Salt (unique per password)
// - 10,000 PBKDF2 iterations
// - SHA256 or SHA512
```

**Password Policy:**

- Minimum 8 characters (configurable)
- Supports alphanumeric and special characters
- API does not enforce complexity, but frontend should

#### 3. **Role-Based Access Control (RBAC)**

ASP.NET Core Identity provides role management:

```csharp
[Authorize]                           // Requires authenticated user
[Authorize(Roles = "Admin")]          // Admin-only access
[AllowAnonymous]                      // Public endpoint
```

**Available Roles:**
| Role | Permissions | Endpoints |
|------|-------------|-----------|
| **Customer** (default) | View products, Create orders, View own orders | GET /products, POST /orders, GET /orders |
| **Admin** | Full CRUD on products, View all orders | All POST/PUT/DELETE on /products, GET /orders |

**Current Authorization Status:**

```
✅ AuthController endpoints: Most public (register, login, send-code, verify-code)
✅ ProductController: Mixed (GET public, CRUD restricted to admin)
✅ OrdersController: Protected (all endpoints require [Authorize])
✅ PaymentsController: Protected (all endpoints require [Authorize])
```

#### 4. **Cross-Origin Resource Sharing (CORS)**

CORS is configured to allow multiple frontend domains:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(
            "https://natural-shop-eta.vercel.app",    // Production
            "https://www.pinararsslan.com",            // Production WWW
            "https://pinararsslan.com",                // Production non-WWW
            "http://localhost:3000",                   // Dev React
            "http://localhost:3001"                    // Dev alternative port
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();  // Important for JWT cookies
    });
});
```

#### 5. **HTTPS & SSL/TLS**

- **Production**: All endpoints must use HTTPS
- **Development**: HTTP allowed on localhost
- **Headers**: Implement HSTS in production

#### 6. **Input Validation & Sanitization**

**Data Annotations (Declarative):**

```csharp
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}

public class SendCodeDto
{
    [RegularExpression(@"^90\d{10}$")]  // Turkish numbering
    public string PhoneNumber { get; set; }
}
```

**Server-Side Validation (Critical):**

- Price recalculation from database (prevents price manipulation)
- Stock validation before order creation
- UserId from JWT token only (not from request body)
- Product existence verification

#### 7. **Payment Security (Iyzipay)**

Secure payment processing with checksums and server-side validation:

```csharp
// 1. Order created with status "Hazırlanıyor"
// 2. Stock reserved during order creation
// 3. Iyzipay checkout form initiated
// 4. Customer completes payment in secure gateway
// 5. Callback webhook verification (with checksum)
// 6. Status updated to "Ödendi" on success
// 7. Stock permanently deducted
```

**Iyzipay Webhook Verification:**

```json
{
  "conversationId": "Unique order ID",
  "status": "success",
  "systemTime": 1609459200000,
  "eventType": "payment.postAuth.completed"
}
```

#### 8. **Security Best Practices Implemented**

| Security Measure         | Status | Details                                 |
| ------------------------ | ------ | --------------------------------------- |
| JWT Token Validation     | ✅     | Signature, issuer, audience, expiration |
| Password Hashing         | ✅     | PBKDF2 with salt via ASP.NET Identity   |
| Server-Side Validation   | ✅     | Prices, stock, user permissions         |
| CORS Restrictions        | ✅     | Specific domains whitelisted            |
| SQL Injection Prevention | ✅     | EF Core parameterized queries           |
| XSS Protection           | ✅     | JSON camelCase output only              |
| Rate Limiting            | ⏳     | Recommended for production              |
| API Key Management       | ⏳     | Store keys in environment variables     |
| Audit Logging            | ✅     | Product updates logged with ChangedAt   |

---

## 📊 Database Schema

### Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────────────────────────────┐
│                       Database Layer                        │
│                    PostgreSQL 12+                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐        ┌──────────────────┐          │
│  │   AspNetUsers    │        │     Products     │          │
│  │  (Accounts)      │        │  (Catalog)       │          │
│  ├──────────────────┤        ├──────────────────┤          │
│  │ Id (PK)          │        │ Id (PK)          │          │
│  │ FullName         │        │ Name             │          │
│  │ Email            │        │ Price            │          │
│  │ PasswordHash     │        │ Stock            │          │
│  │ Address          │        │ Category         │          │
│  │ PhoneNumber      │        │ ImageUrl         │          │
│  └──────────────────┘        │ StoryText        │          │
│           │                  │ StoryImages[]    │          │
│           │ 1:N              │ Images[]         │          │
│           │                  └──────────────────┘          │
│           │                           │                   │
│           │                           │ 1:N               │
│           │                           │                   │
│  ┌–──────┴────────────┐        ┌──────┴──────────────┐   │
│  │     Orders         │        │    OrderItems       │   │
│  │  (Transactions)    │        │  (Line Items)       │   │
│  ├──────────────────┐         ├──────────────────────┤   │
│  │ Id (PK)          │ 1:N     │ Id (PK)              │   │
│  │ UserId (FK)      │────────│ OrderId (FK)         │   │
│  │ CreatedAt        │         │ ProductId (FK)       │   │
│  │ Status           │         │ Quantity             │   │
│  │ TotalAmount      │         │ UnitPrice            │   │
│  │ IyzipayToken     │         │ ProductName          │   │
│  └──────────────────┘         └──────────────────────┘   │
│                                                             │
│  ┌──────────────────┐        ┌──────────────────────┐    │
│  │ VerificationCodes│        │   AspNetRoles        │    │
│  │  (SMS Tokens)    │        │   (RBAC)             │    │
│  ├──────────────────┤        ├──────────────────────┤    │
│  │ Id (PK)          │        │ Id (PK)              │    │
│  │ PhoneNumber      │        │ Name (Admin, User)   │    │
│  │ Code             │        │ NormalizedName       │    │
│  │ CreatedAt        │        └──────────────────────┘    │
│  │ ExpiresAt        │                  │                 │
│  │ IsUsed           │                  │ N:N             │
│  │ UsedAt           │                  │                 │
│  └──────────────────┘        ┌─────────┴─────────────┐   │
│                              │ AspNetUserRoles       │   │
│                              │ (User-Role Mapping)   │   │
│                              ├──────────────────────┤   │
│                              │ UserId (FK)          │   │
│                              │ RoleId (FK)          │   │
│                              └──────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Table Specifications

#### 1. **AspNetUsers (User Accounts)**

| Column                 | Type         | Constraints      | Purpose                                     |
| ---------------------- | ------------ | ---------------- | ------------------------------------------- |
| `Id`                   | VARCHAR(36)  | PK, NOT NULL     | Unique user identifier (GUID)               |
| `FullName`             | TEXT         | NOT NULL         | User's complete name                        |
| `Email`                | VARCHAR(256) | UNIQUE, NOT NULL | Email address                               |
| `NormalizedEmail`      | VARCHAR(256) | UNIQUE           | Uppercase email for case-insensitive lookup |
| `PasswordHash`         | TEXT         | NOT NULL         | PBKDF2 hashed password                      |
| `UserName`             | VARCHAR(256) | UNIQUE           | Login username (same as email)              |
| `Address`              | TEXT         | NULL             | Delivery address                            |
| `PhoneNumber`          | TEXT         | NULL             | Phone number                                |
| `PhoneNumberConfirmed` | BOOLEAN      | DEFAULT false    | Verification status                         |
| `EmailConfirmed`       | BOOLEAN      | DEFAULT false    | Email verification status                   |
| `SecurityStamp`        | TEXT         | NULL             | GUID for Lockout prevention                 |
| `ConcurrencyStamp`     | TEXT         | NULL             | Optimistic concurrency control              |
| `AccessFailedCount`    | INTEGER      | DEFAULT 0        | Failed login attempts                       |
| `LockoutEnabled`       | BOOLEAN      | DEFAULT true     | Account lockout enabled                     |
| `LockoutEnd`           | TIMESTAMPTZ  | NULL             | Lockout expiration timestamp                |

**Indexes:**

```sql
CREATE INDEX idx_users_email ON "AspNetUsers"("NormalizedEmail");
CREATE INDEX idx_users_username ON "AspNetUsers"("NormalizedUserName");
```

---

#### 2. **Products (Product Catalog)**

| Column        | Type          | Constraints         | Purpose                     |
| ------------- | ------------- | ------------------- | --------------------------- |
| `Id`          | INTEGER       | PK, AUTOINCREMENT   | Product identifier          |
| `Name`        | TEXT          | NOT NULL            | Product name                |
| `Description` | TEXT          | NULL                | Long description            |
| `Price`       | NUMERIC(10,2) | NOT NULL            | Price in Turkish Lira (TRY) |
| `Stock`       | INTEGER       | NOT NULL, DEFAULT 0 | Current inventory quantity  |
| `Category`    | TEXT          | NULL                | Product category            |
| `Code`        | TEXT          | NOT NULL, UNIQUE    | SKU/Product code            |
| `ImageUrl`    | TEXT          | NULL                | Primary product image URL   |
| `Images`      | TEXT[]        | NULL                | Gallery image URLs          |
| `StoryText`   | TEXT          | NULL                | Product story/narrative     |
| `StoryImages` | TEXT[]        | NULL                | Story gallery images        |

**Indexes:**

```sql
CREATE INDEX idx_products_category ON "Products"("Category");
CREATE INDEX idx_products_code ON "Products"("Code");
CREATE INDEX idx_products_stock ON "Products"("Stock");
```

**Sample Data:**

```sql
INSERT INTO "Products"
(Name, Category, Price, Stock, Code, Description, ImageUrl)
VALUES
('Ege Sızma Zeytinyağı - Erken Hasat (1L)', 'Zeytinyağı Grubu', 350.00, 100, 'NAT-ZEY-01', '...', '/images/zeytinyagi1.jpg'),
('Geleneksel Odun Ateşi Üzüm Pekmezi', 'Pekmez & Bal', 220.00, 60, 'NAT-PKM-01', '...', '/images/pekmez1.jpg'),
('Pul Biber', 'Baharatlar', 110.00, 150, 'NAT-BBR-01', '...', '/images/tozbiber1.jpg');
```

---

#### 3. **Orders (Customer Orders)**

| Column         | Type          | Constraints                      | Purpose                 |
| -------------- | ------------- | -------------------------------- | ----------------------- |
| `Id`           | INTEGER       | PK, AUTOINCREMENT                | Order identifier        |
| `UserId`       | VARCHAR(36)   | FK → AspNetUsers, NOT NULL       | Customer reference      |
| `CreatedAt`    | TIMESTAMPTZ   | NOT NULL, DEFAULT NOW()          | Order timestamp         |
| `Status`       | TEXT          | NOT NULL, DEFAULT 'Hazırlanıyor' | Order status            |
| `TotalAmount`  | NUMERIC(10,2) | NOT NULL                         | Total order value (TRY) |
| `IyzipayToken` | TEXT          | NULL                             | Payment gateway token   |

**Status Values:**

- `Hazırlanıyor` - Preparing
- `Ödeme Bekleniyor` - Payment Pending
- `Ödendi` - Paid
- `Gönderildi` - Shipped
- `Teslim Edildi` - Delivered
- `İptal Edildi` - Cancelled

**Indexes:**

```sql
CREATE INDEX idx_orders_user ON "Orders"("UserId");
CREATE INDEX idx_orders_created ON "Orders"("CreatedAt" DESC);
CREATE INDEX idx_orders_status ON "Orders"("Status");
```

---

#### 4. **OrderItems (Line Items)**

| Column        | Type          | Constraints             | Purpose               |
| ------------- | ------------- | ----------------------- | --------------------- |
| `Id`          | INTEGER       | PK, AUTOINCREMENT       | Line item identifier  |
| `OrderId`     | INTEGER       | FK → Orders, NOT NULL   | Order reference       |
| `ProductId`   | INTEGER       | FK → Products, NOT NULL | Product reference     |
| `Quantity`    | INTEGER       | NOT NULL, > 0           | Units ordered         |
| `UnitPrice`   | NUMERIC(10,2) | NOT NULL                | Price at order time   |
| `ProductName` | VARCHAR(100)  | NOT NULL                | Product name snapshot |

**Indexes:**

```sql
CREATE INDEX idx_order_items_order ON "OrderItems"("OrderId");
CREATE INDEX idx_order_items_product ON "OrderItems"("ProductId");
```

**Calculation:**

```sql
-- Line item total
SELECT SUM(Quantity * UnitPrice) as LineItemTotal
FROM OrderItems
WHERE OrderId = {orderId}

-- Order total
SELECT SUM(oi.Quantity * oi.UnitPrice) as OrderTotal
FROM OrderItems oi
WHERE oi.OrderId = {orderId}
```

---

#### 5. **VerificationCodes (SMS Tokens)**

| Column        | Type        | Constraints             | Purpose                             |
| ------------- | ----------- | ----------------------- | ----------------------------------- |
| `Id`          | INTEGER     | PK, AUTOINCREMENT       | Code identifier                     |
| `PhoneNumber` | VARCHAR(20) | NOT NULL                | Turkish phone number (90XXXXXXXXXX) |
| `Code`        | VARCHAR(10) | NOT NULL                | 6-digit verification code           |
| `CreatedAt`   | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Generation timestamp                |
| `ExpiresAt`   | TIMESTAMPTZ | NOT NULL                | Expiration time (10 min)            |
| `IsUsed`      | BOOLEAN     | DEFAULT false           | Usage status                        |
| `UsedAt`      | TIMESTAMPTZ | NULL                    | Verification timestamp              |

**Rules:**

- Code valid for 10 minutes
- Cannot send new code within 5 minutes
- Only one valid code per phone number
- Used codes marked with IsUsed flag

**Indexes:**

```sql
CREATE INDEX idx_verification_phone ON "VerificationCodes"("PhoneNumber");
CREATE INDEX idx_verification_unused ON "VerificationCodes"("IsUsed", "ExpiresAt");
```

---

### Relationships Summary

| Relationship              | Type         | Description                        |
| ------------------------- | ------------ | ---------------------------------- |
| Users → Orders            | 1:N          | One user has many orders           |
| Orders → OrderItems       | 1:N          | One order contains many line items |
| Products → OrderItems     | 1:N          | One product appears in many orders |
| Users → VerificationCodes | Implicit N:1 | Codes linked via PhoneNumber       |
| Users → AspNetRoles       | N:N          | User role assignments              |

---

## 🛠️ API Patterns & Best Practices

### RESTful API Design

The API adheres to REST principles with standard HTTP methods and status codes:

| Principle                 | Implementation                                               |
| ------------------------- | ------------------------------------------------------------ |
| **Resource-oriented**     | URLs represent nouns (e.g., `/api/products`, `/api/orders`)  |
| **Standard HTTP Methods** | GET (retrieve), POST (create), PUT (update), DELETE (remove) |
| **Stateless**             | Each request contains all necessary context (JWT token)      |
| **JSON Format**           | Request/response bodies in JSON with camelCase properties    |
| **Versioning**            | Currently v1 (implicit in routes)                            |
| **Error Handling**        | Consistent error responses with codes and messages           |

### DTO (Data Transfer Object) Pattern

DTOs provide a contract between client and server, enabling:

**Benefits:**

- ✅ Data hiding (internal properties not exposed)
- ✅ Validation at API boundary
- ✅ Decoupling from database models
- ✅ API versioning flexibility
- ✅ Backward compatibility during refactoring

**DTO Hierarchy:**

```
Models (Database Entities)
  ↓
DTOs (Transfer Objects)
  ├─ CreateProductDto
  ├─ UpdateProductDto
  ├─ ProductDto
  ├─ CreateOrderDto
  ├─ LoginDto
  ├─ RegisterDto
  ├─ SendCodeDto
  └─ VerifyCodeDto
  ↓
JSON Response
```

**Example DTO Chain:**

```csharp
// 1. Client sends CreateProductDto
POST /api/products
{
  "name": "Olive Oil",
  "price": 350.00,
  "stock": 100
}

// 2. Controller maps to Product entity
var product = _mapper.Map<Product>(createProductDto);
_context.Products.Add(product);
await _context.SaveChangesAsync();

// 3. Response mapped to ProductDto
var response = _mapper.Map<ProductDto>(product);
return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);

// 4. Client receives ProductDto
{
  "id": 1,
  "name": "Olive Oil",
  "price": 350.00,
  "stock": 100,
  "category": "Zeytinyağı Grubu"
}
```

### AutoMapper Configuration

Centralized mapping configuration in `MappingProfile.cs`:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity → DTO
        CreateMap<Product, ProductDto>();

        // DTO → Entity (with custom field handling)
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.Price ?? 0))
            .ForMember(dest => dest.Stock,
                opt => opt.MapFrom(src => src.Stock ?? 0));

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Never update ID
    }
}
```

**Usage:**

```csharp
var productDto = _mapper.Map<ProductDto>(product);           // Single entity
var productDtos = products.ProjectTo<ProductDto>(_mapper);   // LINQ queryable
```

### JSON Serialization & Camel Case

The API automatically converts property names to camelCase for JavaScript compatibility:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Convert PascalCase to camelCase in JSON
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;

        // Handle circular references (JsonIgnore on navigation properties)
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });
```

**Example:**

```csharp
// C# Model
public class Product
{
    public string ProductName { get; set; }
    public decimal TotalPrice { get; set; }
}

// JSON Output
{
  "productName": "Olive Oil",      // PascalCase → camelCase
  "totalPrice": 350.00
}
```

### Dependency Injection

Service layer abstraction with DI container:

```csharp
// Service interface
public interface ISmsService
{
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
}

// Service implementation
public class SmsService : ISmsService
{
    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        // Implementation...
    }
}

// Registration
builder.Services.AddScoped<ISmsService, SmsService>();

// Usage in controller
public class AuthController
{
    private readonly ISmsService _smsService;

    public AuthController(ISmsService smsService)
    {
        _smsService = smsService;
    }
}
```

### Error Handling & Status Codes

Consistent HTTP status code usage:

| Status Code                   | Scenario                  | Example                              |
| ----------------------------- | ------------------------- | ------------------------------------ |
| **200 OK**                    | Successful GET/PUT/DELETE | Product retrieved successfully       |
| **201 Created**               | Resource created (POST)   | New product created with ID returned |
| **400 Bad Request**           | Invalid input data        | Invalid product price or stock       |
| **401 Unauthorized**          | Missing/invalid JWT token | No Authorization header              |
| **403 Forbidden**             | Insufficient permissions  | User trying to access admin endpoint |
| **404 Not Found**             | Resource doesn't exist    | Product ID 999 not found             |
| **409 Conflict**              | Business logic violation  | Ordering more items than in stock    |
| **500 Internal Server Error** | Unhandled exception       | Database connection error            |

**Error Response Format:**

```json
{
  "message": "Sipariş bulunamadı",
  "statusCode": 404,
  "timestamp": "2026-02-08T10:30:00Z"
}
```

### Input Validation

Multi-layer validation approach:

**Layer 1: Data Annotations (Model-level)**

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    public string Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be > 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock >= 0")]
    public int Stock { get; set; }
}
```

**Layer 2: Server-Side Business Logic**

```csharp
// Stock validation
if (product.Stock < item.Quantity)
{
    return BadRequest(new { message = "Insufficient stock" });
}

// Price recalculation from database
var serverPrice = _db.Products.Find(item.ProductId).Price;
if (clientPrice != serverPrice)
{
    // Log security incident, use server price
}
```

**Layer 3: Database Constraints**

```sql
ALTER TABLE "Products"
ADD CONSTRAINT check_price CHECK ("Price" > 0);

ALTER TABLE "OrderItems"
ADD CONSTRAINT check_quantity CHECK ("Quantity" > 0);
```

---

## 🔧 Technology Stack

### Backend Framework & Language

| Technology       | Version | Purpose                      |
| ---------------- | ------- | ---------------------------- |
| **.NET**         | 8.0 LTS | Web framework runtime        |
| **C#**           | 12      | Primary programming language |
| **ASP.NET Core** | 8.0     | Web API framework            |

### Database & ORM

| Technology                | Version | Purpose                         |
| ------------------------- | ------- | ------------------------------- |
| **PostgreSQL**            | 12+     | Relational database             |
| **Entity Framework Core** | 9.0     | ORM for data access             |
| **Npgsql**                | 9.0     | PostgreSQL provider for EF Core |
| **EF Core Migrations**    | 9.0     | Schema versioning & evolution   |

### Security & Authentication

| Technology                | Version | Purpose                            |
| ------------------------- | ------- | ---------------------------------- |
| **ASP.NET Core Identity** | 8.0     | User management & password hashing |
| **JWT Bearer**            | 8.0     | Stateless authentication tokens    |
| **CORS**                  | 2.3     | Cross-origin resource sharing      |
| **Security Policies**     | 8.0     | Authorization attributes           |

### Mapping & Serialization

| Technology                                              | Version    | Purpose                  |
| ------------------------------------------------------- | ---------- | ------------------------ |
| **AutoMapper**                                          | 12.0.1     | Entity ↔ DTO mapping     |
| **AutoMapper.Extensions.Microsoft.DependencyInjection** | 12.0.1     | DI container integration |
| **System.Text.Json**                                    | (Built-in) | JSON serialization       |

### External Integrations

| Service     | Purpose                            | Status        |
| ----------- | ---------------------------------- | ------------- |
| **Iyzipay** | Payment gateway (Visa, Mastercard) | ✅ Integrated |
| **Netgsm**  | SMS verification service           | ⚙️ Configured |
| **Ngrok**   | Webhook tunneling (dev)            | ⚙️ Optional   |

### Development Tools

| Tool                     | Version | Purpose               |
| ------------------------ | ------- | --------------------- |
| **Visual Studio / Code** | Latest  | IDE                   |
| **Swagger/Swashbuckle**  | 6.6.2   | API documentation UI  |
| **Postman/Insomnia**     | Latest  | API testing           |
| **pgAdmin**              | Latest  | PostgreSQL management |

### NuGet Packages (Complete List)

```xml
<!-- Web & Framework -->
Microsoft.AspNetCore.Authentication.JwtBearer v8.0.0
Microsoft.AspNetCore.Cors v2.3.0
Microsoft.AspNetCore.Identity.EntityFrameworkCore v8.0.0
Swashbuckle.AspNetCore v6.6.2

<!-- Database -->
Microsoft.EntityFrameworkCore v9.0.10
Microsoft.EntityFrameworkCore.Tools v9.0.10
Npgsql.EntityFrameworkCore.PostgreSQL v9.0.0

<!-- Mapping -->
AutoMapper.Extensions.Microsoft.DependencyInjection v12.0.1

<!-- Payments -->
Iyzipay v2.1.67

<!-- Utilities -->
Microsoft.Extensions.Http (via DI)
```

### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                  Client Applications                         │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│   │React Frontend│  │Mobile App    │  │External API │      │
│   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
└──────────┼─────────────────┼─────────────────┼────────────────┘
           │                 │                 │
           └─────────────────┼─────────────────┘
                             │ HTTPS
           ┌─────────────────┴──────────────────┐
           │                                    │
      ┌────▼────────────────────────────────────────┐
      │ .NET 8 Web API (ASP.NET Core)              │
      ├───────────────────────────────────────────┤
      │ ┌─────────────────────────────────────┐   │
      │ │   Controllers Layer                 │   │
      │ ├─────────────────────────────────────┤   │
      │ │ • AuthController (JWT)              │   │
      │ │ • ProductController (CRUD)          │   │
      │ │ • OrdersController (Orders)         │   │
      │ │ • PaymentsController (Iyzipay)      │   │
      │ └──────────────┬──────────────────────┘   │
      │                │                          │
      │ ┌──────────────▼──────────────────────┐   │
      │ │   Services Layer                    │   │
      │ ├──────────────────────────────────────┤  │
      │ │ • ISmsService / SmsService          │   │
      │ │ • AutoMapper                        │   │
      │ │ • Business Logic & Validation       │   │
      │ └──────────────┬──────────────────────┘   │
      │                │                          │
      │ ┌──────────────▼──────────────────────┐   │
      │ │   Data Layer (EF Core)              │   │
      │ ├──────────────────────────────────────┤  │
      │ │ • AppDbContext                      │   │
      │ │ • Entity Models & Configurations    │   │
      │ │ • Migrations                        │   │
      │ └──────────────┬──────────────────────┘   │
      └─────────────────┼──────────────────────────┘
                        │
      ┌─────────────────┼──────────────────────────┐
      │                 │                          │
  ┌───▼──────┐  ┌──────▼───┐  ┌─────────────────┐ │
  │PostgreSQL│  │ Iyzipay  │  │Netgsm SMS API   │ │
  │Database  │  │ Gateway  │  │SMS Service      │ │
  └──────────┘  └──────────┘  └─────────────────┘ │
      │                                             │
      └─────────────────────────────────────────────┘
```

---

## 📡 API Endpoints Documentation

### Authentication Endpoints

#### 1. **Register User**

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Ahmet Yılmaz",
  "email": "ahmet@example.com",
  "password": "SecurePass123!"
}
```

**Request DTO:**

```csharp
public class RegisterDto
{
    [Required]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}
```

**Response:**

```json
{
  "message": "Kayıt başarılı ✅"
}
```

| Status  | Response                              |
| ------- | ------------------------------------- |
| **200** | Registration successful               |
| **400** | Invalid input or email already exists |

---

#### 2. **User Login**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "ahmet@example.com",
  "password": "SecurePass123!"
}
```

**Request DTO:**

```csharp
public class LoginDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public string Password { get; set; }
}
```

**Success Response (200):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "d2c4f8a0-1234-4b8c-9f8e-1234567890ab",
    "fullName": "Ahmet Yılmaz",
    "email": "ahmet@example.com"
  }
}
```

**Token Usage:**

```http
GET /api/products HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Error Responses:**

```json
// 401 - Invalid credentials
{
  "message": "Kullanıcı bulunamadı ❌"
}

// 401 - Wrong password
{
  "message": "Şifre hatalı ❌"
}
```

---

#### 3. **Send SMS Verification Code**

```http
POST /api/auth/send-code
Content-Type: application/json

{
  "phoneNumber": "905551234567"
}
```

**Request DTO:**

```csharp
public class SendCodeDto
{
    [Required]
    [RegularExpression(@"^90\d{10}$",
        ErrorMessage = "Turkish phone format required: 90XXXXXXXXXX")]
    public string PhoneNumber { get; set; }
}
```

**Response (Success 200):**

```json
{
  "message": "Doğrulama kodu gönderildi."
}
```

**Error Responses:**

```json
// 400 - Rate limited (< 5 min since last code)
{
  "message": "Lütfen 120 saniye sonra tekrar deneyin."
}

// 400 - Invalid phone format
{
  "message": "Geçerli bir telefon numarası giriniz (90XXXXXXXXXX formatında)"
}
```

**Development Mode:**
When SMS credentials aren't configured, codes print to console:

```
=== SMS KODU (Development) ===
Telefon: 905551234567
Kod: 123456
Süre: 2026-02-08 10:30:00
=============================
```

---

#### 4. **Verify SMS Code** _(Not implemented yet)_

```http
POST /api/auth/verify-code
Content-Type: application/json

{
  "phoneNumber": "905551234567",
  "code": "123456"
}
```

**Request DTO:**

```csharp
public class VerifyCodeDto
{
    [Required]
    [RegularExpression(@"^90\d{10}$")]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string Code { get; set; }
}
```

---

### Product Endpoints

#### 5. **List All Products**

```http
GET /api/product
Accept: application/json
```

**Response (200):**

```json
[
  {
    "id": 1,
    "name": "Ege Sızma Zeytinyağı - Erken Hasat (1L)",
    "description": "0.8 asit oranına sahip, soğuk sıkım...",
    "price": 350.00,
    "stock": 100,
    "category": "Zeytinyağı Grubu",
    "imageUrl": "/images/zeytinyagi1.jpg",
    "storyImages": [
      "/images/zeytinyagi2.jpg",
      "/images/zeytinyagi3.jpg"
    ]
  },
  {
    "id": 2,
    "name": "Geleneksel Odun Ateşi Üzüm Pekmezi",
    "price": 220.00,
    "stock": 60,
    "category": "Pekmez & Bal",
    "imageUrl": "/images/pekmez1.jpg",
    "storyImages": [...]
  }
]
```

---

#### 6. **Get Product by ID**

```http
GET /api/product/{id}
Accept: application/json
```

**Example:** `GET /api/product/1`

**Response (200):**

```json
{
  "id": 1,
  "name": "Ege Sızma Zeytinyağı - Erken Hasat (1L)",
  "description": "0.8 asit oranına sahip...",
  "price": 350.00,
  "stock": 100,
  "category": "Zeytinyağı Grubu",
  "imageUrl": "/images/zeytinyagi1.jpg",
  "storyImages": [...]
}
```

**Error (404):**

```json
{
  "message": "Product not found"
}
```

---

#### 7. **Create Product** (Admin only)

```http
POST /api/product
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "name": "Premium Sıvı Yağ 1L",
  "description": "Doğal sıvı yağ",
  "price": 450.00,
  "stock": 50,
  "category": "Sıvı Yağlar",
  "code": "NAT-SY-01",
  "imageUrl": "/images/siviyag.jpg",
  "storyText": "Geleneksel yöntemle...",
  "storyImages": ["/images/siviyag2.jpg"]
}
```

**Request DTO:**

```csharp
public class CreateProductDto
{
    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }

    public string? Category { get; set; }
    public string Code { get; set; }
    public string? ImageUrl { get; set; }
    public string? StoryText { get; set; }
    public string[]? StoryImages { get; set; }
}
```

**Response (201 Created):**

```json
{
  "id": 5,
  "name": "Premium Sıvı Yağ 1L",
  "price": 450.0,
  "stock": 50,
  "category": "Sıvı Yağlar",
  "imageUrl": "/images/siviyag.jpg"
}
```

**Error Responses:**

```json
// 400 - Validation error
{
  "message": "Geçersiz veri gönderildi",
  "errors": [
    {
      "field": "Price",
      "error": "Price must be greater than 0"
    }
  ]
}

// 401 - Unauthorized (no token)
{
  "message": "Unauthorized"
}

// 403 - Forbidden (not admin)
{
  "message": "Forbidden"
}
```

---

#### 8. **Update Product** (Admin only)

```http
PUT /api/product/{id}
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "name": "Updated Product Name",
  "price": 400.00,
  "stock": 75,
  "category": "Updated Category"
}
```

**Response (200):**

```json
{
  "id": 1,
  "name": "Updated Product Name",
  "price": 400.00,
  "stock": 75,
  "category": "Updated Category",
  ...
}
```

**Logging:**
The update operation logs old and new values:

```
[INFO] Updating product — ID: 1, Old Name: Original, Old Price: 350, Old Stock: 100
[INFO] Product updated successfully — ID: 1, New Name: Updated, New Price: 400, New Stock: 75
```

---

#### 9. **Delete Product** (Admin only)

```http
DELETE /api/product/{id}
Authorization: Bearer {jwt_token}
```

**Response (204 No Content):**

```
(Empty body)
```

**Error (404):**

```json
{
  "message": "Product not found"
}
```

---

### Order Endpoints

#### 10. **Get User's Orders**

```http
GET /api/orders
Authorization: Bearer {jwt_token}
```

**Response (200):**

```json
[
  {
    "id": 1,
    "createdAt": "2026-02-08T10:30:00Z",
    "status": "Ödendi",
    "totalAmount": 700.0,
    "items": [
      {
        "id": 1,
        "orderId": 1,
        "productId": 1,
        "productName": "Ege Sızma Zeytinyağı",
        "quantity": 2,
        "unitPrice": 350.0
      }
    ]
  }
]
```

**Authorization:** Requires JWT token. Returns only user's own orders.

---

#### 11. **Get Order by ID**

```http
GET /api/orders/{id}
Authorization: Bearer {jwt_token}
```

**Response (200):**

```json
{
  "id": 1,
  "createdAt": "2026-02-08T10:30:00Z",
  "status": "Ödendi",
  "totalAmount": 700.00,
  "userId": "d2c4f8a0-1234-4b8c-9f8e-1234567890ab",
  "items": [...]
}
```

**Error (404):**

```json
{
  "message": "Sipariş bulunamadı"
}
```

---

#### 12. **Create Order (Prepare for Payment)**

```http
POST /api/orders
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 3,
      "quantity": 1
    }
  ]
}
```

**Request DTO:**

```csharp
public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
```

**Response (200):**

```json
{
  "message": "Sipariş oluşturuldu ✅"
}
```

**Error Responses:**

```json
// 401 - User not authenticated
{
  "message": "Kullanıcı kimliği bulunamadı"
}

// 404 - Product not found
{
  "message": "Ürün bulunamadı: 999"
}

// 409 - Insufficient stock
{
  "message": "Ege Sızma Zeytinyağı için yeterli stok yok. Mevcut stok: 50"
}

// 400 - Invalid quantity
{
  "message": "Ürün için geçersiz miktar: -5"
}
```

---

### Payment Endpoints

#### 13. **Start Payment (Get Checkout Form)**

```http
POST /api/payments/start
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

**Process:**

1. Validate user from JWT token
2. Verify products exist
3. Check stock availability
4. Calculate total (server-side)
5. Create Iyzipay checkout form token
6. Return token to frontend
7. Frontend displays Iyzipay checkout modal

**Response (200):**

```json
{
  "token": "P1rVJu6RZiVJ2K4L8M9N0O",
  "checkoutFormContent": "<iframe src=\"https://sandbox.iyzipay.com/...\"></iframe>",
  "orderId": 42,
  "totalAmount": 700.0
}
```

**Error Responses:**

```json
// 400 - Validation error
{
  "message": "Geçersiz veri gönderildi",
  "errors": [...]
}

// 401 - Unauthorized
{
  "message": "Kullanıcı kimliği bulunamadı"
}

// 404 - User or product not found
{
  "message": "Kullanıcı bulunamadı"
}

// 409 - Stock issue
{
  "message": "Ürün stoğu yetersiz"
}
```

---

#### 14. **Payment Callback Webhook** _(Iyzipay endpoint)_

```http
POST /api/payments/callback
Content-Type: application/json

{
  "conversationId": "order-id-42",
  "status": "success",
  "systemTime": 1609459200000,
  "eventType": "payment.postAuth.completed"
}
```

**Process:**

1. Verify webhook signature (Iyzipay checksum)
2. Update order status to "Ödendi"
3. Deduct stock from inventory (atomically)
4. Log payment confirmation

---

### Endpoints Summary Table

| Method     | Endpoint                 | Auth | Role   | Status |
| ---------- | ------------------------ | ---- | ------ | ------ |
| **POST**   | `/api/auth/register`     | ❌   | All    | ✅     |
| **POST**   | `/api/auth/login`        | ❌   | All    | ✅     |
| **POST**   | `/api/auth/send-code`    | ❌   | All    | ✅     |
| **POST**   | `/api/auth/verify-code`  | ❌   | All    | ⏳     |
| **GET**    | `/api/product`           | ❌   | All    | ✅     |
| **GET**    | `/api/product/{id}`      | ❌   | All    | ✅     |
| **POST**   | `/api/product`           | ✅   | Admin  | ✅     |
| **PUT**    | `/api/product/{id}`      | ✅   | Admin  | ✅     |
| **DELETE** | `/api/product/{id}`      | ✅   | Admin  | ✅     |
| **GET**    | `/api/orders`            | ✅   | User   | ✅     |
| **GET**    | `/api/orders/{id}`       | ✅   | User   | ✅     |
| **POST**   | `/api/orders`            | ✅   | User   | ✅     |
| **POST**   | `/api/payments/start`    | ✅   | User   | ✅     |
| **POST**   | `/api/payments/callback` | ❌   | System | ✅     |

---

## 💾 Database Setup & Migrations

### Prerequisites

- **PostgreSQL 12+** installed and running
- **pgAdmin** or PostgreSQL CLI
- **.NET 8 SDK**

### Connection String

**Location:** `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NaturalShopDB;Username=postgres;Password=postgres;"
  }
}
```

**Format Breakdown:**

```
Host=localhost              — Database server address
Port=5432                   — PostgreSQL default port
Database=NaturalShopDB      — Database name
Username=postgres           — Username
Password=postgres           — Password (⚠️ Change in production!)
```

**For Production:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.azure.com;Port=5432;Database=prod_naturalshop;Username=admin@server;Password=SecurePass123!;"
  }
}
```

### Migration History

| Migration   | File                             | Date        | Changes                                                                                  |
| ----------- | -------------------------------- | ----------- | ---------------------------------------------------------------------------------------- |
| **Initial** | `20260108083315_InitPostgres.cs` | Jan 8, 2026 | Create tables: AspNetUsers, AspNetRoles, Products, Orders, OrderItems, VerificationCodes |

### Running Migrations

#### **Option 1: Using dotnet CLI**

```bash
# Update database to latest migration
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

#### **Option 2: Using Package Manager Console** (Visual Studio)

```powershell
# PowerShell in Visual Studio Package Manager Console
Update-Database

Add-Migration MigrationName

Remove-Migration
```

#### **Option 3: Automatic during App Startup**

The application auto-applies migrations:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();  // Auto-apply pending migrations
}}
```

### Database Initialization

#### **Automatic Seeding on Startup**

The application seeds sample products on first run:

```csharp
// Program.cs
DbInitializer.Seed(app);
SeedData.InitializeAsync(context);
```

**DbInitializer adds:**

- ~12 natural products (olive oil, dried fruits, spices, honey, etc.)
- Product categories
- Stock quantities
- Product images and stories

#### **Manual Seeding**

```sql
-- Add sample products
INSERT INTO "Products"
(Name, Category, Price, Stock, Code, Description, ImageUrl)
VALUES
('Ege Sızma Zeytinyağı - Erken Hasat (1L)', 'Zeytinyağı Grubu', 350.00, 100, 'NAT-ZEY-01',
 '0.8 asit oranına sahip, soğuk sıkım tekniğiyle üretilmiş', '/images/zeytinyagi1.jpg');
```

### Backing Up the Database

#### **Using PostgreSQL `pg_dump`**

```bash
# Backup to file
pg_dump -U postgres -h localhost -d NaturalShopDB > backup.sql

# Restore from backup
psql -U postgres -h localhost -d NaturalShopDB < backup.sql
```

#### **Using pgAdmin GUI**

1. Right-click database → Backup
2. Select format (Plain, Custom, TAR)
3. Choose backup filename

### Troubleshooting Migrations

**Issue:** `The migration '20250108083315_InitPostgres' is not applied to the database you are trying to connect to.`

**Solution:**

```bash
# Reset database (development only!)
dotnet ef database drop --force
dotnet ef database update
```

**Issue:** `Errors detected during conversion. The 'Products' table failed to build.`

**Solution:** Check column constraints and data types match between C# model and migration.

---

## 🚀 Development Setup

### System Requirements

| Component          | Minimum                                 | Recommended                          |
| ------------------ | --------------------------------------- | ------------------------------------ |
| Operating System   | Windows 10 / macOS 10.15 / Ubuntu 20.04 | Windows 11 / macOS 12 / Ubuntu 22.04 |
| RAM                | 4 GB                                    | 8 GB                                 |
| Disk Space         | 2 GB                                    | 5 GB                                 |
| .NET SDK           | 8.0                                     | 8.0 LTS                              |
| PostgreSQL         | 12                                      | 14+                                  |
| Node.js (frontend) | 16                                      | 18+                                  |

### Installation Steps

#### **Step 1: Install Prerequisites**

```bash
# Verify .NET installation
dotnet --version

# Install PostgreSQL (Windows)
# Download from https://www.postgresql.org/download/windows/
# Or via: choco install postgresql

# Verify PostgreSQL
psql --version
```

#### **Step 2: Clone Repository**

```bash
git clone https://github.com/48pinaribs/NaturalShop.API.git
cd NaturalShop.API
```

#### **Step 3: Restore Dependencies**

```bash
# Restore NuGet packages
dotnet restore
```

#### **Step 4: Configure Database**

```bash
# Create database (PostgreSQL)
psql -U postgres -c "CREATE DATABASE NaturalShopDB;"

# Or via pgAdmin: Create new database named "NaturalShopDB"
```

#### **Step 5: Apply Migrations**

```bash
dotnet ef database update
```

**Expected Output:**

```
DbInitializer başlatılıyor...
✅ DbInitializer başarıyla tamamlandı.
```

#### **Step 6: Run Development Server**

```bash
dotnet run

# Server output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7261
#       Now listening on: http://localhost:5261
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to shut down.
```

#### **Step 7: Access API Documentation**

Open browser:

- **Swagger UI:** https://localhost:7261/swagger/index.html
- **API Root:** https://localhost:7261/api

### Development Configuration

**appsettings.Development.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  },
  "JwtSettings": {
    "Key": "dev-secret-key-12345",
    "Issuer": "NaturalShop",
    "Audience": "NaturalShopUsers"
  },
  "Iyzipay": {
    "BaseUrl": "https://sandbox-api.iyzipay.com",
    "ApiKey": "sandbox-key",
    "SecretKey": "sandbox-secret"
  }
}
```

### IDE Setup

#### **Visual Studio 2022**

```bash
# Open solution
start NaturalShop.API.sln
```

**Extensions Recommended:**

- REST Client (for testing)
- NuGet Package Manager
- SQL Server Object Explorer
- Azure Tools

#### **Visual Studio Code**

```bash
# Install extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension ms-mssql.mssql
code --install-extension eamodio.gitlens

# Open folder
code .
```

**Launch Configuration (`.vscode/launch.json`):**

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/NaturalShop.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "console": "integratedTerminal"
    }
  ]
}
```

---

## 📦 Deployment Guide

### Docker Deployment

**Dockerfile included in repository:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NaturalShop.API.dll"]
```

**Build and run:**

```bash
# Build image
docker build -t naturalshop-api:latest .

# Run container
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=NaturalShopDB;Username=postgres;Password=postgres;" \
  naturalshop-api:latest
```

### Azure App Service Deployment

```bash
# Install Azure CLI
# https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

# Login to Azure
az login

# Create resource group
az group create --name NaturalShopRG --location eastus

# Create App Service plan
az appservice plan create \
  --name NaturalShopPlan \
  --resource-group NaturalShopRG \
  --sku B2 \
  --is-linux

# Create web app
az webapp create \
  --name naturalshop-api \
  --resource-group NaturalShopRG \
  --plan NaturalShopPlan \
  --runtime "DOTNETCORE:8.0"

# Deploy from GitHub
az webapp deployment source config-zip \
  --resource-group NaturalShopRG \
  --name naturalshop-api \
  --src app.zip
```

### Environment Variables (Production)

```bash
# Set in App Service Configuration
ConnectionStrings__DefaultConnection=<connection-string>
JwtSettings__Key=<strong-random-key>
Iyzipay__ApiKey=<api-key>
Iyzipay__SecretKey=<secret-key>
ASPNETCORE_ENVIRONMENT=Production
```

### CI/CD Pipeline (GitHub Actions)

**Example `.github/workflows/deploy.yml`:**

```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "8.0.x"

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

      - name: Publish
        run: dotnet publish -c Release -o ./publish

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: naturalshop-api
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ./publish
```

---

## 📝 Development Best Practices

### Code Organization

```
Controllers/          # HTTP request handlers
├─ AuthController.cs
├─ ProductController.cs
├─ OrdersController.cs
└─ PaymentsController.cs

Models/              # Entity classes (database models)
├─ ApplicationUser.cs
├─ Product.cs
├─ Order.cs
├─ OrderItem.cs
└─ VerificationCode.cs

DTOs/                # Data Transfer Objects
├─ RegisterDto.cs
├─ LoginDto.cs
├─ CreateProductDto.cs
├─ UpdateProductDto.cs
├─ ProductDto.cs
├─ CreateOrderDto.cs
├─ SendCodeDto.cs
└─ VerifyCodeDto.cs

Services/            # Business logic
├─ ISmsService.cs
└─ SmsService.cs

Data/                # Database context
├─ AppDbContext.cs
├─ DbInitializer.cs
└─ SeedData.cs

Migrations/          # EF Core migrations
└─ 20260108083315_InitPostgres.cs

Mappings/            # AutoMapper profiles
└─ MappingProfile.cs

Helpers/             # Utility classes
└─ IyzipayConfig.cs
```

### Logging Strategy

```csharp
// Injected ILogger<T>
private readonly ILogger<AuthController> _logger;

// Log levels
_logger.LogInformation("User {UserId} logged in", userId);
_logger.LogWarning("SMS sending failed: {Reason}", reason);
_logger.LogError(ex, "Payment processing error for OrderId: {Id}", orderId);
```

### Error Handling Pattern

```csharp
try
{
    // Business logic
}
catch (ArgumentException ex)
{
    _logger.LogWarning("Invalid argument: {Message}", ex.Message);
    return BadRequest(new { message = ex.Message });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unhandled exception");
    return StatusCode(500, new { message = "Internal server error" });
}
```

### Async/Await Pattern

```csharp
// Always use async for I/O operations
public async Task<IActionResult> GetProducts()
{
    var products = await _context.Products
        .AsNoTracking()
        .ToListAsync();
    return Ok(products);
}
```

---

## 📞 Support & Contact

- **Repository:** [48pinaribs/NaturalShop.API](https://github.com/48pinaribs/NaturalShop.API)
- **Issues:** GitHub Issues tab
- **Documentation:** This README

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Last Updated:** February 8, 2026 | **.NET 8** | **PostgreSQL 12+** | **Production Ready**
