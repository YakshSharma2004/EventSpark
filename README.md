# EventSpark 🎟️

EventSpark is an ASP.NET Core MVC web application for managing events and tickets.

- Organizers can create events, define ticket types, and check in attendees.
- Attendees can browse events, purchase tickets, and view their orders/tickets.
- Admins get a simple dashboard with totals for events, tickets, revenue, and users.

The solution is structured into multiple projects with clear separation between
domain, data access, web UI, and tests.

---

## 🚀 Tech Stack

- **Language:** C# (.NET 8)
- **Framework:** ASP.NET Core MVC
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Auth:** ASP.NET Core Identity
- **UI:** Razor Views + Bootstrap 5 + custom CSS
- **QR Codes:** [QRCoder](https://github.com/codebude/QRCoder)
- **Testing:** xUnit + EF Core InMemory

---

## 🧱 Solution Structure

- `EventSpark.Core`
  - Domain entities and enums (Event, TicketType, Order, Ticket, etc.)
- `EventSpark.Infrastructure`
  - `EventSparkDbContext` (inherits from `IdentityDbContext<ApplicationUser>`)
  - EF Core setup, DbSets, relationships
- `EventSpark.Web`
  - MVC web app, controllers, view models, Razor views, CSS/JS
- `EventSpark.Tests`
  - xUnit tests using EF Core InMemory provider

There are also SQL scripts in the repository:

- `creation.sql` – creates the **EventSpark** domain tables (Events, TicketTypes, Orders, Tickets, CheckInLogs, etc.)
- `usersadd.sql` – creates ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.) if they do not exist.

---

## ✅ Prerequisites

You’ll need:

- **.NET 8 SDK**
- **SQL Server** (LocalDB or full SQL Server)
  - e.g. `MSSQLLocalDB` that ships with Visual Studio
- **Visual Studio 2022** (recommended)  
  – with “ASP.NET and web development” workload  
  or
- **VS Code + C# Dev Kit** (optional alternative)
- **SQL Server Management Studio (SSMS)** or another SQL client to run scripts

---

## 📥 Getting the Code

```bash
git clone <your-repo-url> EventSpark
cd EventSpark
