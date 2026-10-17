# Legacy integration (WCF and .NET Framework)

## Why it exists

Many shops still run an older warehouse or ERP system on .NET Framework that exposes WCF/SOAP services.
SkirtPling simulates one: a **legacy warehouse** that knows the physical stock count per SKU. Admins can
compare the web shop's stock with the warehouse count on **Admin → Legacy warehouse**.

It is deliberately small and kept out of the main code path: shopping, checkout and admin CRUD work the
same whether or not the legacy service is running.

## Components

| Component | Runtime | What it does |
|---|---|---|
| `backend/LegacyIntegration/WcfService` | .NET Framework 4.8 | Self-hosted WCF service, `BasicHttpBinding`, `IWarehouseService.GetStock(sku)`. WSDL at `?wsdl`. |
| `backend/SkirtPling.DAL/External/LegacyWarehouseClient.cs` | .NET 10 | WCF **client** (`System.ServiceModel.Http`) used by the modern API. |
| `backend/SkirtPling.BL/Services/LegacyInventoryService.cs` | .NET 10 | Business logic: looks up the shop product and compares counts. Sees only `ILegacyWarehouseClient`. |
| `backend/LegacyIntegration/NetFrameworkClient` | .NET Framework 4.8 | Console app showing the opposite direction: an old .NET Framework program consuming the new REST API (and the WCF service) to print a reconciliation report. |

```
Angular ──REST──► SkirtPling.Api ──► LegacyInventoryService ──► ILegacyWarehouseClient
                                                                   │ SOAP over HTTP
NetFrameworkClient (net48) ──REST──► SkirtPling.Api                ▼
NetFrameworkClient (net48) ──SOAP────────────────────────────► WcfService (net48)
```

The .NET Framework code never runs inside the ASP.NET Core process. The two runtimes only talk over
HTTP, so they share **no assemblies**. The WCF contract is duplicated in the modern client on purpose;
the XML namespace (`http://skirtpling.example/legacy/warehouse`), contract and member names must match.

## Running it

Requirements: **Windows** with .NET Framework 4.8 (included in Windows 10 1903+ and Windows 11) to host
the service. Both net48 projects also *build* on macOS/Linux with the .NET SDK (reference assemblies), but
WCF hosting needs Windows.

```powershell
cd backend/LegacyIntegration
dotnet build LegacyIntegration.sln

# 1. Start the WCF service (listens on http://localhost:8733/LegacyWarehouse)
.\WcfService\bin\Debug\net48\SkirtPling.Legacy.WcfService.exe
```

If you see *"HTTP could not register URL"*, either run the terminal as Administrator once or reserve the
URL for your user:

```powershell
netsh http add urlacl url=http://+:8733/LegacyWarehouse user=$env:USERNAME
```

```powershell
# 2. Point the API at it (user-secrets, environment variable, or appsettings.Development.json)
cd ..\SkirtPling.Api
dotnet user-secrets set "LegacyWarehouse:Endpoint" "http://localhost:8733/LegacyWarehouse"
dotnet run

# 3. In the Angular app: sign in as admin → Admin → Legacy warehouse → enter SP-PL-001

# 4. Optional: the .NET Framework client report (API and WCF service must be running)
..\LegacyIntegration\NetFrameworkClient\bin\Debug\net48\SkirtPling.Legacy.NetFrameworkClient.exe http://localhost:5080
```

When `LegacyWarehouse:Endpoint` is empty the admin endpoint returns `503 Legacy warehouse is not
configured`; when the service is down it returns `503 The legacy warehouse service is not responding`.

## Deployment notes

- Host the WCF service on a Windows server (as a Windows Service, or in IIS with a `.svc` file) and expose
  it only on the internal network. `BasicHttpBinding` has no transport security; use HTTPS
  (`BasicHttpSecurityMode.Transport`) outside a trusted network.
- The modern API can run anywhere (Linux containers included); it only needs network access to the WCF URL.

## How it was verified

In the development container (Linux) the net48 projects were built with the .NET 10 SDK and run under Mono
to exercise the wire protocol: the .NET 10 API successfully called `GetStock` and returned the comparison,
and the net48 console printed the reconciliation report from the live REST API and WCF service. Running on
real .NET Framework on Windows was **not** tested in that environment.
