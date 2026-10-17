# Architecture

SkirtPling is a classic three-layer .NET application with a separate Angular single-page app, plus an
isolated legacy component that shows .NET Framework ↔ modern .NET interoperability.

```
 Angular SPA (frontend/)            REST + JSON, JWT bearer
 ───────────────────────────────►  SkirtPling.Api  (presentation: HTTP only)
                                         │ calls service interfaces
                                         ▼
                                    SkirtPling.BL   (business rules, DTOs, mapping)
                                         │ calls repository / gateway interfaces
                                         ▼
                                    SkirtPling.DAL  (EF Core, Identity store, external systems)
                                     │          │
                              SQL Server     SOAP/HTTP (BasicHttpBinding)
                                                │
                                   LegacyIntegration/WcfService  (.NET Framework 4.8)
```

## Projects and dependencies

| Project | Target | References | Responsibility |
|---|---|---|---|
| `SkirtPling.Api` | net10.0 | BL, DAL | Controllers, auth (JWT), error handling, CORS, health checks, startup. |
| `SkirtPling.BL` | net10.0 | DAL | Business rules: pricing, stock checks, checkout orchestration, order status rules, account rules. |
| `SkirtPling.DAL` | net10.0 | – | `AppDbContext`, entity configurations, migrations, repositories, seed data, payment gateway and WCF client. |
| `SkirtPling.Tests` | net10.0 | all three | BL tests on in-memory SQLite and API integration tests via `WebApplicationFactory`. |
| `LegacyIntegration/WcfService` | net48 | – | Self-hosted WCF "legacy warehouse" service. |
| `LegacyIntegration/NetFrameworkClient` | net48 | (links the WCF contract file) | .NET Framework console that calls both the WCF service and the REST API. |

The API references DAL only to register it (`AddDataAccess`) and to use Identity types; controllers
never touch repositories or `AppDbContext`.

## Rules each layer follows

**API (`SkirtPling.Api`)**
- Controllers only bind input, call one BL service, and return a result. No business logic.
- `[ApiController]` validates DTO data annotations automatically and returns `400` with field errors.
- `Middleware/AppExceptionHandler` maps BL exceptions to RFC 7807 problem responses:
  `NotFoundException → 404`, `BusinessRuleException → 400`, `ConflictException → 409`,
  `ServiceUnavailableException → 503`, anything else `→ 500` with a generic message (details only in logs).
- All admin controllers inherit `AdminControllerBase`, which carries `[Authorize(Roles = "Admin")]`.
- User ids always come from the token (`User.GetUserId()`), never from the URL or body.

**BL (`SkirtPling.BL`)**
- No ASP.NET Core / HTTP types and no EF Core calls; it talks to the DAL through interfaces.
- DTOs (`DTOs/`) are the API contract. Entities are never returned to callers; `Mapping/` converts them.
- `PricingCalculator` is the single source of subtotal and shipping rules (used by cart and checkout).
- `TimeProvider` is injected so time-dependent code is testable.

**DAL (`SkirtPling.DAL`)**
- One repository per aggregate with intention-revealing methods (no generic repository).
- EF configuration lives in `Configurations/`, never as attributes on entities.
- `External/` holds adapters to systems outside the database: `IPaymentGateway` (mock sandbox) and
  `ILegacyWarehouseClient` (WCF). The BL only sees the interfaces.

**Frontend (`frontend/src/app`)**
- `core/` app-wide singletons: models (API contracts), API services, auth/cart state, interceptor, guards, layout.
- `shared/` reusable presentational components (product card, pagination, badges, dialogs) and small utils.
- `features/` one folder per area (catalog, product details, cart, checkout, account, admin), lazy-loaded.
- Components never call `HttpClient` directly; they use a service from `core/services`.
- State is kept in signals inside a few services (`AuthService`, `CartService`); there is no global store.

## Key flows

### Cart (guest and signed-in)
The browser creates a random guest id (`GuestCartIdService`) and the interceptor sends it as
`X-Guest-Cart-Id` on every call. Anonymous requests use the guest cart; authenticated requests use the
user's cart. Right after login/registration the client calls `POST /api/cart/merge`, which moves guest
lines into the user cart (capped at available stock). Prices in the cart are always read from the
product table on the server.

### Checkout
`OrderService.PlaceOrderAsync`:
1. Loads the user's cart and re-checks every product is active and in stock.
2. Builds the order from **server** prices and calculates totals with `PricingCalculator`.
3. `OrderRepository.CreateWithStockReservationAsync` runs one DB transaction: a conditional
   `UPDATE … SET Stock = Stock - n WHERE Stock >= n` per line, then inserts the order as `PendingPayment`.
   If any line can't be reserved, everything rolls back and the shopper gets `409`.
4. Calls `IPaymentGateway.ChargeAsync` with a provider token (never card data).
5. Approved → order becomes `Paid`, cart is emptied. Declined → order is `Cancelled` and stock is returned
   in a transaction.

### Order status
Allowed admin transitions live in `OrderService.AllowedTransitions`
(`PendingPayment → Cancelled`, `Paid → Shipped | Cancelled`, `Shipped → Delivered`). Cancelling restocks.

### Authentication
ASP.NET Core Identity stores users and hashes passwords (PBKDF2). `AccountService` validates credentials
and lockout (5 failures → 10 minutes); the API's `JwtTokenService` issues a signed JWT (HS256, 2 hours)
with `sub`, `email`, `name` and role claims. The Angular app stores the token in `localStorage`.

## Differences from the original brief

- **Naming:** the brief's example used `BrandStore`; the solution is named after the repository (`SkirtPling`).
- **No `BL/Validators` folder:** input validation is done with data annotations on the request DTOs and
  business validation inside the services; a separate validator layer would have been empty indirection.
- **Repository interfaces live in the DAL** next to their implementations (BL → DAL dependency), which is
  the conventional direction for a three-layer design and keeps each repository in one file.
- **Payment and WCF adapters live in `DAL/External`**: they are I/O boundaries like the database.
