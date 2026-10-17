# SkirtPling

A full-stack e-commerce store for branded skirts: ASP.NET Core Web API (.NET 10) with a clean three-layer
backend, SQL Server through EF Core, an Angular 21 storefront and admin area, and an isolated
.NET Framework/WCF component that shows legacy interoperability.

- **Storefront:** home page, catalog with search, filters (brand, category, price, availability), sorting and
  paging, product page with gallery, specifications and related products, Add to cart and Buy now.
- **Cart and checkout:** guest and signed-in carts (merged on sign-in), server-side price and stock checks,
  checkout with shipping address and a sandbox payment gateway, order confirmation.
- **Accounts:** registration, sign-in (JWT), profile, saved addresses, order history.
- **Admin:** dashboard (revenue, orders, low stock, top sellers), product CRUD with images and specs,
  inline stock edits, brands and categories, order status management, user roles and locking,
  legacy warehouse stock comparison.

More detail: [docs/architecture.md](docs/architecture.md) · [docs/legacy-integration.md](docs/legacy-integration.md) · [docs/deploy-render-neon.md](docs/deploy-render-neon.md) (free hosting)

## Folder structure

```
skirtPling/
├── backend/
│   ├── SkirtPling.sln
│   ├── SkirtPling.Api/            HTTP only: Controllers/ (+ Admin/), Middleware/, Extensions/, Auth/, Program.cs
│   ├── SkirtPling.BL/             Business logic: Interfaces/, Services/, DTOs/, Mapping/, Exceptions/
│   ├── SkirtPling.DAL/            Data access: Entities/, Context/, Configurations/, Repositories/, Migrations/, Seed/, External/
│   ├── SkirtPling.Tests/          Unit/ (business rules), Integration/ (API), TestSupport/
│   └── LegacyIntegration/         Separate solution, .NET Framework 4.8
│       ├── WcfService/            Self-hosted WCF "legacy warehouse"
│       └── NetFrameworkClient/    net48 console calling the WCF service and the REST API
├── frontend/
│   ├── scripts/                   Generator for the demo product images
│   ├── public/images/             Product images (SVG)
│   └── src/
│       ├── app/
│       │   ├── core/              models, API services, auth/cart state, interceptor, guards, layout
│       │   ├── shared/            reusable components and utils
│       │   ├── features/          home, catalog, product-details, cart, checkout, account, admin
│       │   └── app.routes.ts
│       └── environments/
├── docs/
├── Dockerfile, render.yaml        One-container deployment (API + Angular) for Render
├── docker-compose.yml             Optional SQL Server for macOS/Linux
└── README.md
```

## Prerequisites

| Tool | Version used | Notes |
|---|---|---|
| .NET SDK | 10.0 | `dotnet --list-sdks` |
| Node.js | 22.12+ (or 24) | Angular 21 requirement |
| SQL Server | LocalDB, Express, Developer or Docker | LocalDB ships with Visual Studio on Windows |
| Windows + .NET Framework 4.8 | optional | only to *run* the legacy WCF service |

## Quick start

### 1. Database

**Windows with LocalDB** (default): nothing to do. `appsettings.Development.json` points to
`(localdb)\MSSQLLocalDB` and the API creates the database on first run.

**macOS / Linux / Docker:** start SQL Server and tell the API where it is:

```bash
docker compose up -d
cd backend/SkirtPling.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=SkirtPling;User Id=sa;Password=Dev_Passw0rd!;TrustServerCertificate=True"
```

In Development the API applies EF Core migrations and seeds the demo data at startup
(`Database:InitializeOnStartup`). To do it by hand instead:

```bash
cd backend
dotnet tool restore
dotnet ef database update -p SkirtPling.DAL -s SkirtPling.Api
```

### 2. Backend

```bash
cd backend
dotnet run --project SkirtPling.Api        # http://localhost:5080
```

Check it: `http://localhost:5080/health` → `Healthy`, `http://localhost:5080/api/products`.
The OpenAPI document is at `http://localhost:5080/openapi/v1.json` in Development.

### 3. Frontend

```bash
cd frontend
npm ci
npm start                                  # http://localhost:4200
```

### 4. Sign in

| Account | How |
|---|---|
| Admin | `admin@skirtpling.local` / `Admin123!dev` (seeded **in Development only**, from `appsettings.Development.json`) |
| Customer | Register on the site |

At checkout choose **Test card: payment approved** or **declined**; no card details are collected.

## Configuration and secrets

All settings are in `backend/SkirtPling.Api/appsettings.json`; Development overrides are in
`appsettings.Development.json`. Override any value with user-secrets or environment variables
(`ConnectionStrings__Default`, `Jwt__Key`, …).

| Key | Purpose | Development value |
|---|---|---|
| `ConnectionStrings:Default` | SQL Server connection | LocalDB |
| `Database:Provider` | `SqlServer` (default, with migrations), `Postgres` (hosting on Neon) or `Sqlite` (tests); the last two create the schema without migrations | `SqlServer` |
| `Database:InitializeOnStartup` | Apply migrations and seed at startup | `true` |
| `Jwt:Key` | HMAC signing key, ≥ 32 chars. **The API refuses to start without it.** | dev-only key |
| `Jwt:ExpiryMinutes` | Token lifetime | 120 |
| `Cors:AllowedOrigins` | Origins allowed to call the API | `http://localhost:4200` |
| `Seed:SeedCatalog`, `Seed:AdminEmail`, `Seed:AdminPassword` | Demo data and dev admin | enabled |
| `LegacyWarehouse:Endpoint` | WCF service URL; empty switches the integration off | empty |

`appsettings.json` (production defaults) contains no secrets: no connection string, no JWT key, no admin
password and seeding off. In production set these through environment variables or a secret store.
The development JWT key and admin password are committed on purpose so the project runs out of the box;
never reuse them outside your machine.

## Database and migrations

- EF Core 10 with SQL Server; schema in `SkirtPling.DAL/Configurations`, migrations in `SkirtPling.DAL/Migrations`.
- Add a migration after changing entities:
  ```bash
  cd backend
  dotnet ef migrations add <Name> -p SkirtPling.DAL -s SkirtPling.Api -o Migrations
  ```
- Seed data (`SkirtPling.DAL/Seed`) is deterministic: 5 brands, 5 categories, 20 products with images
  and specifications, plus the roles `Admin` and `Customer`. It only inserts when the tables are empty.
- Reset the dev database: `dotnet ef database drop -p SkirtPling.DAL -s SkirtPling.Api`, then run the API.

## Tests

```bash
cd backend && dotnet test                      # unit + integration
cd frontend && npm test -- --watch=false       # Vitest
```

Results from the last run (in this repository's development container):

| Suite | Result |
|---|---|
| Backend (`dotnet test`) | **41 passed**, 0 failed |
| Frontend (`ng test`) | **12 passed**, 0 failed |
| `dotnet build` (main solution and `LegacyIntegration.sln`) | succeeded, 0 warnings |
| `ng build` (production) | succeeded |

What is covered:
- **Business rules (BL against in-memory SQLite):** pricing and free-shipping threshold, cart quantity/stock
  limits, inactive products, guest-cart merge, checkout success, payment decline (stock restored, cart kept),
  stock sold out during checkout (no charge), order ownership, order status transitions and restocking.
- **API integration (`WebApplicationFactory`):** anonymous → `401` and customer → `403` on admin endpoints,
  customers can't read other customers' orders, guest cart → sign-in merge → checkout end-to-end,
  validation errors, duplicate SKU `409`, brand-in-use `409`, hidden products, health check.
- **Frontend:** URL ↔ filter mapping, API error messages, stock badge, cart service and interceptor headers,
  route guards.

Besides the automated tests, the full flow (browse, filter, cart, register, declined and approved checkout,
order history, admin dashboard, product edit, order shipping, users, legacy lookup, mobile layout) was
clicked through in a headless browser against SQL Server 2022 with no console errors.

## API endpoints

All routes are under `/api`. 🔒 = signed in, 👑 = Admin role.

| Method | Route | Description |
|---|---|---|
| GET | `/products` | Search: `search`, `brandIds`, `categoryIds`, `minPrice`, `maxPrice`, `inStockOnly`, `sort` (`newest`, `price-asc`, `price-desc`, `name`), `page`, `pageSize` |
| GET | `/products/{id}` | Details with images, specs and related products |
| GET | `/brands`, `/categories` | Lookup lists |
| GET | `/cart` | Current cart (user, or guest via `X-Guest-Cart-Id` header) |
| POST | `/cart/items` | Add `{ productId, quantity }` |
| PUT / DELETE | `/cart/items/{productId}` | Change quantity / remove |
| POST 🔒 | `/cart/merge` | Move the guest cart into the user's cart |
| POST | `/auth/register`, `/auth/login` | Returns `{ token, expiresAt, user }` |
| GET / PUT 🔒 | `/account/profile` | Own profile |
| GET / POST 🔒 | `/account/addresses` | Saved addresses |
| PUT / DELETE 🔒 | `/account/addresses/{id}` | Edit / delete own address |
| POST 🔒 | `/orders` | Checkout `{ shippingAddress, paymentToken }` |
| GET 🔒 | `/orders`, `/orders/{id}` | Own orders only |
| GET 👑 | `/admin/dashboard` | Statistics |
| GET / POST 👑 | `/admin/products` | List (includes hidden; extra sort `stock`) / create |
| GET / PUT / DELETE 👑 | `/admin/products/{id}` | Read / update / delete (refused if the product was ever ordered) |
| PATCH 👑 | `/admin/products/{id}/stock` | Set stock |
| POST, PUT / DELETE 👑 | `/admin/brands`, `/admin/brands/{id}` | Brand CRUD (delete refused while in use) |
| POST, PUT / DELETE 👑 | `/admin/categories`, `/admin/categories/{id}` | Category CRUD (delete refused while in use) |
| GET 👑 | `/admin/orders`, `/admin/orders/{id}` | Search by status / number / email |
| PATCH 👑 | `/admin/orders/{id}/status` | `{ status }` within allowed transitions |
| GET 👑 | `/admin/users` | Search users |
| PUT 👑 | `/admin/users/{id}/access` | `{ isAdmin, isLocked }`; not allowed on yourself |
| GET 👑 | `/admin/legacy-inventory/status`, `/admin/legacy-inventory/{sku}` | Legacy warehouse comparison |
| GET | `/health` (no `/api`) | Health check including the database |

Errors are returned as RFC 7807 problem details (`title`, `detail`, and `errors` for validation).

## Deployment

`Dockerfile` builds one image in which the API also serves the Angular app. `render.yaml` deploys it to
Render's free tier with a Neon PostgreSQL database; see [docs/deploy-render-neon.md](docs/deploy-render-neon.md).
That image was tested locally against PostgreSQL 17 with the same browser walkthrough.

## Known limitations and simplifications

- **Payments are simulated.** `MockPaymentGateway` accepts the test tokens `tok_test_approved` /
  `tok_test_declined`. A real provider would be a new `IPaymentGateway` implementation using the
  provider's hosted card fields so card data never touches this server. Refunds are not implemented;
  cancelling a paid order only restocks.
- **JWT in `localStorage`, no refresh tokens.** Simple and common for SPAs, but vulnerable if an XSS bug
  exists. A locked user's existing token stays valid until it expires (2 hours).
- **Checkout requires an account.** Guests can shop and build a cart; they sign in or register at checkout.
- **Product images are URLs.** Admins manage the image list by URL; there is no file upload or storage service.
- **No email** (order confirmation, password reset, email confirmation).
- **Single currency (ILS) and one flat shipping rule** (₪29.90, free from ₪300) in `PricingCalculator`.
- The legacy WCF service was verified on Mono in the Linux dev container, not on Windows (see
  [docs/legacy-integration.md](docs/legacy-integration.md)).

## Where to extend first

1. **A real payment provider** behind `IPaymentGateway` (e.g. Stripe or an Israeli provider in test mode),
   with webhooks to confirm payment and a refund step when an admin cancels a paid order.
2. **Image upload** for products (admin form → API → blob storage) instead of typing URLs.
3. **Refresh tokens in an HttpOnly cookie** to replace the long-lived JWT in `localStorage`.
4. **Transactional emails** (order confirmation, shipping notice, password reset).
5. **CI pipeline** running `dotnet test` and `ng test` on every pull request.
