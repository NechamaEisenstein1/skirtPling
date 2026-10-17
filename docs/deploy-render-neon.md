# Free hosting on Render + Neon

The site runs as **one Docker web service on Render** (the ASP.NET Core API also serves the built Angular
app) with a **PostgreSQL database on Neon**. Both have free tiers. The legacy WCF component is not deployed
(it needs Windows).

## How it works

- `Dockerfile` (repo root) builds Angular, publishes the API and copies the Angular build into `wwwroot`.
  Browser and API share one origin, so no CORS setup is needed.
- `render.yaml` is a Render Blueprint describing that service and its environment variables.
- With `Database__Provider=Postgres` the API uses Npgsql and creates the schema on first start
  (`EnsureCreated`), then seeds the demo catalog and the admin account. The SQL Server migrations are not
  used for PostgreSQL.
- Neon's connection URL (`postgresql://user:password@host/db?sslmode=require`) can be pasted as-is; the API
  converts it to Npgsql format.

## Steps

1. **Neon:** create a free account at https://neon.tech, create a project (choose the region closest to
   your Render region, e.g. Frankfurt), and copy the connection string from *Connect*.
2. **Render:** create a free account at https://render.com and connect your GitHub account.
3. *New → Blueprint*, choose the `skirtPling` repository. Render reads `render.yaml`.
4. Fill in the three secret values Render asks for:
   - `ConnectionStrings__Default`: the Neon connection string
   - `Seed__AdminEmail`: the admin email you want
   - `Seed__AdminPassword`: at least 8 characters with an uppercase letter, a lowercase letter and a digit
5. *Apply*. The first build takes several minutes. When it is live, open `https://<service>.onrender.com`.
   `/health` should say `Healthy`.

Every push to the deployed branch redeploys automatically.

## Free tier limits (check the providers' current pricing pages)

- Render free web services **sleep after ~15 minutes without traffic**; the next visit waits roughly a minute
  while it wakes. Free instance hours per month are limited.
- Neon free projects have limited storage (about 0.5 GB) and the database also suspends when idle, adding
  a short delay to the first query.
- Payments stay in sandbox mode; no real charges are possible.

## Changing the schema later

PostgreSQL uses `EnsureCreated`, which only creates tables in an empty database. After changing entities,
either reset the Neon database (Neon → branch → reset/delete tables) or add a dedicated PostgreSQL
migrations project.
