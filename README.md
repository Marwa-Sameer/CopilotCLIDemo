# CopilotCLIDemo

## Main idea
This project is a multi-user household needs manager that allows users to add, update, delete, and view household items so they can be ordered later. It supports individual accounts with roles, categorization of items, and exportable shopping lists.

## Technologies
- Backend:
  - .NET 10
  - C#
- Frontend (MVC):
  - ASP.NET MVC (server-rendered Razor views)
  - HTML
  - CSS
  - JavaScript
- Database:
  - Microsoft SQL Server

---

## Preview of current README
The README describes the project's main idea and core technologies. It now includes a short interview summary (below) that captures the project's intended scope and configuration based on the maintainer's answers.

---

## Interview answers (maintainer)
1) Primary goal: A) Simple personal household list (CRUD) to track needs and order manually, expanded to support multiple users and roles (selected: A)
2) Primary users: C) Multiple users with individual accounts and roles (selected: C)
3) Authentication/authorization: C) Individual accounts with registration, login, and roles (selected: C)
4) Data fields for each item (selected: D):
   - Name
   - Quantity
   - Urgent flag
   - Estimated price
   - Category
   - Vendor / Store
   - Notes

5) Ordering: B) Export lists to CSV/printable formats (selected: B)
6) UI preference: A) Traditional server-rendered MVC pages and forms (selected: A)
7) Production database: A) Microsoft SQL Server (selected: A)
8) Deployment method: A) Run locally / IIS on Windows Server (selected: A)
9) Automated testing level: D) End-to-end tests (UI) as well (selected: D)
10) License & contribution model: A) MIT License — open to public contributions (selected: A)

---

## Project configuration summary
From the answers above, here is the intended scope and recommended initial plan:

- Architecture
  - ASP.NET MVC server-rendered application.
  - .NET 10 backend with controllers and Razor views.
  - Microsoft SQL Server for persistent storage.
  - Authentication with individual user accounts, role-based authorization (e.g., Owner/Admin/Member).

- Core data model (Item / Need)
  - Id (int, PK)
  - Name (string, required)
  - Quantity (int)
  - IsUrgent (bool)
  - EstimatedPrice (decimal, optional)
  - Category (string or FK to Category table)
  - Vendor (string)
  - Notes (string / nvarchar(max))
  - CreatedByUserId (FK to Users)
  - CreatedAt, UpdatedAt (timestamps)

- Features to implement first
  1. User registration, login, and role management.
  2. CRUD for items (server-rendered MVC views and forms).
  3. Export shopping lists to CSV and a printable view.
  4. Basic UI to mark items as urgent and filter/sort by category, vendor or urgency.
  5. End-to-end tests for critical flows (register/login, create/edit/delete item, export list).
  6. Deployment instructions for IIS + SQL Server.

- Non-functional
  - Licensing: MIT (open to contributions).
  - Local development: SQL Server Express or Dockerized SQL Server for parity with production.
  - CI: Add a pipeline that runs unit and end-to-end tests before merges.

---

## Next steps I can help with (pick any)
- Scaffold the data model and EF Core migrations for the Item entity and Categories.
- Add authentication (ASP.NET Identity) and role scaffolding.
- Create the initial MVC controllers and Razor views for item CRUD and list export.
- Add end-to-end tests (Playwright) for the main user flows.
- Add deployment instructions for IIS and SQL Server, or Docker compose if you prefer.

If you tell me which of the next steps you want first, I'll implement it (create files, scaffold code, or open issues) and update the README with more detailed setup and usage instructions.