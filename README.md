# CopilotCLIDemo

## Main idea
This is a simple project to add, update, delete, and view household needs so they can be ordered later. The backend is built with .NET 10 and C#, the frontend uses ASP.NET MVC with HTML, CSS, and JavaScript, and Microsoft SQL Server is used for the database.

## Technologies
- Backend:
  - .NET 10
  - C#
- Frontend (MVC):
  - HTML
  - CSS
  - JavaScript
- Database:
  - Microsoft SQL Server

---

## Preview of current README
The current README contains a short description of the project's idea and lists the core technologies (backend, frontend, database). It can be expanded with more detail about features, setup, usage, data model, deployment, testing, and contribution guidelines.

---

## Interview (multiple-choice)
Please answer each question by selecting exactly one option (A, B, C, or D). If you choose D (Other), briefly describe the details on the line after the question.

1) What is the primary goal of this project?
- A) Simple personal household list (CRUD) to track needs and order manually.
- B) Inventory + shopping list with pricing and budgeting features.
- C) Multi-user shared household lists with permissions and history.
- D) Other (please specify):

2) Who are the primary users?
- A) Single individual managing their own household.
- B) Multiple household members sharing a single account.
- C) Multiple users with individual accounts and roles (owner/admin/member).
- D) Other (please specify):

3) What level of authentication/authorization do you want?
- A) No authentication (open to anyone).
- B) Basic authentication (single shared password or PIN).
- C) Individual accounts with registration, login, and roles.
- D) OAuth/social login (Google, Microsoft, etc.).

4) What data fields should each "need"/item include?
- A) Name, quantity, and an urgent flag.
- B) Name, quantity, estimated price, vendor/store.
- C) Name, quantity, category, notes, optional image.
- D) Other (please specify):

5) How should ordering be handled?
- A) No ordering — generate lists to order manually.
- B) Export lists to CSV/printable formats.
- C) Integrate with an online store or ordering API.
- D) Generate shopping routes or grouped orders for efficiency.

6) What user interface do you prefer?
- A) Traditional server-rendered MVC pages and forms (current approach).
- B) Single-page app (React/Vue) for a richer UX.
- C) Mobile-first responsive site with progressive enhancement.
- D) Command-line or desktop client.

7) Which database/deployment environment do you plan to use for production?
- A) Microsoft SQL Server (on-premises or VM).
- B) Azure SQL / managed cloud DB.
- C) SQLite or local DB for simple deployments/testing.
- D) Other (please specify):

8) How should we deploy this app?
- A) Run locally / IIS on Windows Server.
- B) Docker containers.
- C) Azure App Service or similar PaaS.
- D) Other (please specify):

9) What level of automated testing is required?
- A) No automated tests for now.
- B) Unit tests for backend logic and models.
- C) Integration tests covering data access and controllers.
- D) End-to-end tests (UI) as well.

10) License and contribution model
- A) MIT License — open to public contributions.
- B) Apache-2.0 — open to public contributions.
- C) Proprietary / private repo — no external contributions.
- D) Other (please specify):

---

After you answer these questions, I will update the README to include your selected options as a project configuration summary and optionally add setup and usage instructions tailored to your answers. If you'd like, I can also create issues or a project board to track features based on your responses.