# Free hosting on Render + Azure SQL Database

The site runs as **one Docker web service on Render** (the ASP.NET Core API also serves the built Angular
app) with the **Azure SQL Database free offer** as the database, so production uses the same SQL Server
engine and EF Core migrations as development. The legacy WCF component is not deployed (it needs Windows).

## How it works

- `Dockerfile` (repo root) builds Angular, publishes the API and copies the Angular build into `wwwroot`.
  Browser and API share one origin, so no CORS setup is needed.
- `render.yaml` is a Render Blueprint describing that service and its environment variables.
- On start the API applies the EF Core migrations, then seeds the demo catalog and the admin account
  (only into empty tables).
- SQL Server connections retry transient errors for up to about a minute, which covers the free Azure
  database waking up from auto-pause.

## 1. Azure SQL Database (free offer)

1. Create an Azure account at https://azure.microsoft.com/free. Azure usually asks for a **credit card to
   verify identity**; the free database is not charged as long as auto-pause (step 5) stays selected.
2. Open https://aka.ms/azuresqlhub → *Create a database* → **Start free**. Check that the
   "Free offer applied!" banner is shown.
3. *Basics*: create a new resource group (e.g. `skirtpling-rg`) and a database named `SkirtPling`.
   Next to *Server* choose **Create new**: a globally unique server name, the region closest to your Render
   region (e.g. West Europe for Frankfurt), and **Use SQL authentication** with an admin login and a strong
   password. Keep them; they go into the connection string.
4. *Networking*: **Public endpoint**. Leave "Allow Azure services and resources to access this server" off;
   Render's addresses are added in section 3.
5. *Behavior when free limit reached*: **Auto-pause the database until next month**. Never choose
   "Continue using database for additional charges" (it can't be switched back).
6. Check that the cost summary shows 0, then **Review + create** → **Create**.
7. Database → *Settings → Connection strings* → **ADO.NET (SQL authentication)**. Copy it, replace
   `{your_password}` with the password and change `Connection Timeout=30` to `Connection Timeout=60` so
   the first connection can wait while a paused database wakes up.

## 2. Render

1. Create an account at https://render.com and connect your GitHub account.
2. *New → Blueprint* → choose the `skirtPling` repository and the branch to deploy. Render reads
   `render.yaml`.
3. Enter the three secret values:
   - `ConnectionStrings__Default`: the Azure connection string from above
   - `Seed__AdminEmail`: the admin email you want
   - `Seed__AdminPassword`: at least 8 characters with an uppercase letter, a lowercase letter and a digit
4. **Deploy Blueprint**. The first deploy fails to reach the database until section 3 is done; that's expected.

## 3. Let Render through the Azure firewall

Azure SQL blocks all addresses by default, and "Allow Azure services" does **not** cover Render.

1. Render → your service → **Connect** (top right) → **Outbound** tab: copy the listed IP ranges
   (CIDR notation, e.g. `216.24.60.0/24`).
2. Azure portal → your SQL **server** (not the database) → *Security → Networking* → *Firewall rules* →
   **Add a firewall rule** for each range. For a `/24` range like `216.24.60.0/24` the start IP is
   `216.24.60.0` and the end IP is `216.24.60.255`; for a single address use it as both. → **Save**.
3. Render → **Manual Deploy → Deploy latest commit**. When it is live, open `https://<service>.onrender.com`;
   `/health` should say `Healthy`.

Every push to the deployed branch redeploys automatically.

## Free tier limits (check the providers' current pages)

- **Render:** free web services sleep after ~15 minutes without traffic; the next visit waits roughly a
  minute while it wakes. Free instance hours per month are limited.
- **Azure SQL free offer:** per subscription, up to 10 databases, each with a monthly allowance of
  100,000 vCore-seconds of compute and 32 GB of storage. The database auto-pauses when idle and resumes on
  the next connection (the first request after a pause is slow). With the setting from step 1.5, when the
  monthly allowance runs out the database stays paused until the next month instead of charging you, and
  the site shows errors until then.
- Payments stay in sandbox mode; no real charges are possible.

