# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Building

This is an ASP.NET **Web Site** project (not an SDK-style project with a `.csproj`) — `Paymaker.sln` references the root folder directly via `WebsiteProperties`, and pages under `App_Code/` are compiled dynamically by ASP.NET at runtime rather than by MSBuild. Open `Paymaker.sln` in Visual Studio and use IIS/IIS Express or the built-in VS Development Server to run it; there is no CLI build script.

- Target framework: .NET Framework 4.8 (Web Forms)
- NuGet packages are restored from `packages.config` into `packages/`; compiled dependencies also live directly in `bin/` (some, like `Bootstrap.dll` — the `BootstrapWrapper` controls library — are not NuGet packages and must be present in `bin/` directly)
- No npm/front-end build step — `package-lock.json` is present but there's no `package.json`/build pipeline; JS/CSS are static files under `include/`

## Architecture

Paymaker is commission-management software for real estate sales staff (sales commissions, payroll, campaigns/marketing spend, leave). It shares the same architectural lineage as the JellisCraig `Gears` app (same `G`/`Root`/`DB` pattern, same `BootstrapWrapper` custom controls), but is a separate site with its own database and no ORM-style `DataFrameWork` layer — data access is plain inline SQL.

### Database
Single SQL Server database, configured in `web.config`:
- Connection string key `DB` → `PaymakerLive` (placeholder `DBNAME` in the app-setting is substituted via `Client.DBName` in `DB.DBConn`)

### Core Infrastructure (`App_Code/`)
- **`G`** (`Global.cs`) — Static singleton for session/app state: current user (`G.User`), pay period info (`G.PayPeriodInfo`), commission tier/type caches, delegate info, and app settings (`G.Settings`, backed by `web.config` app settings plus a `CONFIG` DB table loaded into session). Nested `G.User` and `G.Settings` classes hold most of what pages read.
- **`Root`** (`root.cs`) — Base `System.Web.UI.Page` all pages inherit (`public partial class X : Root`). Handles no-cache headers, redirecting unauthenticated users to SAML login (`/acs/samllogin.aspx`), page-dirty tracking, injecting the menu, and start/end JS via `sbStartJS`/`sbEndJS`. Set `blnRestrictPageToAdmin = true` to gate a page to `RoleID == 2`.
- **`DB`** (`DB.cs`) — Static SQL helper (via `Microsoft.ApplicationBlocks.Data.SqlHelper` plus hand-rolled `SqlConnection`/`SqlTransaction` wrappers) with retry logic on `runReader`/`runDataSet`/`runNonQuery`. Use `DB.escape()` for inline SQL string sanitization; `DB.readInt`/`readDouble`/`readDate`/`readString`/`readBool` for null-safe DataRow/reader access. No typed field-tracking ORM — unlike Gears' `DataFrameWork`, updates are hand-written SQL strings.
- **`Security`** (`Security.cs`) — Role-based permissions via the `RolePermissionType` enum; check with `G.User.hasPermission(RolePermissionType.X)`.

### Page Patterns
- Pages are Web Forms `.aspx`/`.aspx.cs`/`.aspx.designer.cs` triplets, organized by module folder rather than a detail/update suffix convention: `main/` (sales & dashboards), `payroll/`, `reports/` (the bulk of business reports), `admin/` (admin tools, imports, audit/error logs), `campaign/` (marketing campaign management), `boxdice/` (BoxDice CRM import/integration), `automation/` (scheduled email jobs), `web_services/` (`.asmx` SOAP endpoints: `ws_Paymaker`, `ws_BoxDice`).
- JS library links are centralized in `HTML.addJSLinks()` (`App_Code/HTML.cs`), not scattered per-page — when bumping a cache-busted local script (e.g. `include/utility.js?v=13`, `include/jquery.mods.js?v=14`), update the version query string in that one method rather than searching individual `.aspx` files.

### Authentication
- Primary: Azure AD SSO via SAML2 (`acs/` — `SAMLLogin.aspx`, `ConsumerService.aspx`, `logout.aspx`), using `ComponentPro.Saml`
- `Root.OnLoad` redirects any request without a session `USERID` to `/acs/samllogin.aspx` (skipped when `G.Settings.Env == "dev"`, i.e. `web.config`'s `env` app setting)
- `login_hidden.aspx` is exempt from the login redirect (used for delegate/impersonation-style flows)

### Reports
`reports/` contains the bulk of business report pages (commission statements, cash flow, campaign aging/prepayment, EOFY bonus, payroll reconciliation, etc.) as self-contained `.aspx`/`.aspx.cs` pairs rather than separate RDLC report definitions — PDF generation uses `iTextSharp`/`FlexCel` rather than ReportViewer RDLCs (though `Microsoft.ReportViewer.WebForms` is referenced in `web.config` for the pages that do use it).

### External Integrations
- **BoxDice** — CRM system integrated via `boxdice/import.aspx` and `ws_BoxDice.cs`/`ws_BoxDice.asmx`
- **MYOB** — accounting export (`main/myob_export.aspx`, `campaign/myob_export.aspx`, `admin/myob_*`)
- **Sentry** — error tracking, initialized in `Global.asax.cs` `Application_Start` from the `SentryDNS` app setting
