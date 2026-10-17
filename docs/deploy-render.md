# Free hosting on Render + Azure SQL Database

The site runs as **one Docker web service on Render** (the ASP.NET Core API also serves the built Angular
app) with the **Azure SQL Database free offer** as the database, so production uses the same SQL Server
engine and EF Core migrations as development. The legacy WCF component is not deployed (it needs Windows).

Prefer no Azure account? The same image also runs on **Neon (PostgreSQL)**; see the last section.

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
   verify identity**; the free database offer itself is not charged as long as you keep the setting in step 4.
2. In the Azure portal: *Create a resource → SQL Database*. On the creation page, click
   **Apply offer** on the "Want to try Azure SQL Database for free?" banner.
3. Create a new server with **SQL authentication**: choose an admin login and a strong password, and the
   region closest to your Render region (e.g. West Europe for Frankfurt).
4. Under the free offer settings choose **"Auto-pause the database until next month"** when the free limit
   is used up. Do *not* choose "continue using the database for additional charges".
5. *Networking*: public endpoint. Firewall rules are added in step 3 below.
6. After it is created: database → *Connection strings* → **ADO.NET (SQL authentication)**. Copy it and
   replace `{your_password}` with your password. Add `Connection Timeout=60;` at the end so the first
   connection can wait while a paused database wakes up.

## 2. Render

1. Create an account at https://render.com and connect your GitHub account.
2. *New → Blueprint* → choose the `skirtPling` repository. Render reads `render.yaml`.
3. Enter the three secret values:
   - `ConnectionStrings__Default`: the Azure connection string from above
   - `Seed__AdminEmail`: the admin email you want
   - `Seed__AdminPassword`: at least 8 characters with an uppercase letter, a lowercase letter and a digit
4. *Apply*. The first deploy will fail to reach the database until step 3 is done; that's expected.

## 3. Let Render through the Azure firewall

Azure SQL blocks all addresses by default, and "Allow Azure services" does **not** cover Render.

1. Render → your service → *Connect* → *Outbound* tab: copy the listed outbound IP ranges/addresses.
2. Azure portal → your SQL **server** → *Networking* → *Firewall rules* → add one rule per Render address
   (start = end for single IPs) → *Save*.
3. Render → *Manual Deploy → Deploy latest commit*. When it is live, open `https://<service>.onrender.com`;
   `/health` should say `Healthy`.

Every push to the deployed branch redeploys automatically.

## Free tier limits (check the providers' current pages)

- **Render:** free web services sleep after ~15 minutes without traffic; the next visit waits roughly a
  minute while it wakes. Free instance hours per month are limited.
- **Azure SQL free offer:** per subscription, up to 10 databases, each with a monthly allowance of
  100,000 vCore-seconds of compute and 32 GB of storage. The database auto-pauses when idle and resumes on
  the next connection (the first request after a pause is slow). With the setting from step 1.4, when the
  monthly allowance runs out the database stays paused until the next month instead of charging you, and
  the site shows errors until then.
- Payments stay in sandbox mode; no real charges are possible.

## Alternative: Neon (PostgreSQL), no credit card

1. Create a free project at https://neon.tech and copy its connection string (`postgresql://…`).
2. In Render, set `Database__Provider` to `Postgres` and paste the Neon string into
   `ConnectionStrings__Default` (the API converts the URL format itself). No firewall step is needed.

With PostgreSQL the schema is created with `EnsureCreated` instead of migrations, which only works on an
empty database. After changing entities, reset the Neon database or add PostgreSQL migrations.
Limits: about 0.5 GB storage; the database suspends when idle.
