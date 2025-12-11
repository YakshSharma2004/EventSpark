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
```
---
##🗄️ Database Setup

The project assumes a SQL Server database that matches the schema from the
creation.sql and usersadd.sql scripts.

1. Create the database + tables

 -Open SQL Server Management Studio (SSMS).

 -Connect to your local SQL Server (for example: (localdb)\MSSQLLocalDB).

 -Open the file creation.sql from the repo.

 -Check the very top of the script:
---
```sql
-- 1. Create database (optional if you already created it)
IF DB_ID('EventSpark') IS NULL
BEGIN
    CREATE DATABASE EventSparkDb;
END
GO

USE EventSpark;
GO

```
-Either:

--Rename the database in your connection string to match this (EventSpark), or

--Adjust the script so that both the DB_ID(...) and USE ... lines use the same name as your connection string (e.g. EventSparkDb).

-Run the script.
--This will create the Event / Ticket / Order / CheckIn tables.

2. Create Identity tables (users/roles)

In SSMS, open usersadd.sql.

Make sure the USE statement at the top points to the same database name you used above:
```sql
USE EventSpark;    -- or EventSparkDb, to match your setup
GO
```

Execute the script.
This will create the ASP.NET Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.) if they are missing.

Note: no default users are inserted by the script. You will register users through the UI when you run the app.

##🔧 Configure the Connection String

In EventSpark.Web/appsettings.json, locate the ConnectionStrings section:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EventSpark;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Adjust it if needed:

-- Server= – your SQL Server instance
e.g. (localdb)\\MSSQLLocalDB or localhost, .\SQLEXPRESS, etc.

-- Database= – must match the DB you created / used in the scripts
(e.g. EventSpark or EventSparkDb).

--If you prefer environment-specific configuration, you can override this via
appsettings.Development.json or user secrets.

## ▶️ Running the Application
Option 1: Visual Studio

-- Set EventSpark.Web as the startup project (right-click → Set as Startup Project).

-- Make sure build configuration is Debug.

-- Press F5 (or click the ▶ Run button).

-- The app will start and open in your browser, usually at a URL like:
```bash
https://localhost:7281/
```

The app is configured so that the first meaningful page is the login / register screen
(under /Identity/Account/Login), or the public /Events list depending on your configuration.

Option 2: .NET CLI

From the solution root:
```bash
dotnet build
dotnet run --project EventSpark.Web
```

Then browse to the URL shown in the console output (e.g. https://localhost:7281).

##👤 Creating Users & Admin

Run the application.

Go to Register (e.g. /Identity/Account/Register).

Create a normal test user (this can be used as an attendee and organizer).

Making an Admin (optional)

If you want an admin for the dashboard:

Register a user in the UI.

In SSMS, find that user’s Id in AspNetUsers table.

Insert an Admin role if it doesn’t exist:
```sql
INSERT INTO AspNetRoles (Id, Name, NormalizedName)
VALUES ('00000000-0000-0000-0000-000000000001', 'Admin', 'ADMIN');
```

Link the user to the Admin role:
```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<your-user-id-here>', '00000000-0000-0000-0000-000000000001');
```

Log out and sign back in as that user.
You should now see the Admin link in the navbar.

##🧪 Running Tests

The solution includes an EventSpark.Tests project with xUnit tests.

Visual Studio

Open Test Explorer

Click Run All Tests

.NET CLI

From the solution root:
```bash
dotnet test
```

Tests use EF Core’s InMemory provider and do not require a real SQL Server
database.

##🧭 Quick Feature Walkthrough

Once the app is running and your DB is configured:

Register & login.

Go to My Events and create an event.

Add Ticket Types for your event (e.g. General Admission, VIP).

Publish the event (set status to Published).

Visit Events:

Filter/search to find your event.

Click Buy to purchase tickets.

Complete the mock checkout:

Select quantities.

Confirm & pay (no real payment gateway).

View My Orders and My Tickets to see the order and generated tickets.

As an organizer:

Use Check In to validate a ticket by pasting its code.

As an Admin (optional):

Open Dashboard → Admin to see totals for events, tickets sold, revenue, and users.

##🐞 Troubleshooting

Q: I get a SQL connection error on startup.

Check that SQL Server / LocalDB is installed and running.

Verify the connection string in appsettings.json (server name & database name).

Q: Running creation.sql fails at USE EventSpark.

Make sure the database name in the script matches the DB you created.

Update USE ... line, or create a DB called EventSpark.

Q: Identity tables already exist.

If you already ran migrations or another script, usersadd.sql may not be needed.

If it’s failing on create table, you can safely comment out the conflicting parts.

Q: I don’t see the Admin link in the navbar.

Confirm that:

The Admin role exists in AspNetRoles.

The current user has a row in AspNetUserRoles with that role ID.
