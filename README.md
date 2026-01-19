# Household Needs Manager

A multi-user household needs management system that allows users to add, update, delete, and view household items for efficient shopping list management. The application supports individual user accounts with roles, multi-household participation, categorization of items, and exportable shopping lists.

## Features

### Multi-Household Support
- **Personal Items**: Create and manage your own private shopping lists
- **Household Items**: Share shopping lists with household members
- **Multiple Households**: Join and manage multiple households simultaneously
- **Role-Based Access**: Owner, Admin, and Member roles with different permissions
- **Easy Switching**: Quickly switch between households with a dropdown menu

### Item Management
- **Full CRUD Operations**: Create, read, update, and delete items
- **Rich Item Details**: Name, quantity, category, vendor, price, urgency, and notes
- **Smart Filtering**: Filter by personal/household, category, urgency, and search by name
- **CSV Export**: Export shopping lists to CSV for easy sharing and printing
- **Printable Lists**: Generate printer-friendly shopping lists

### Dashboard & Analytics
- **Current Week Statistics**: View total items, urgent items, and estimated costs
- **Historical Trends**: Track shopping patterns over the last 4 weeks
- **Visual Charts**: Interactive Chart.js visualizations for category distribution and trends
- **Dual Context**: Separate statistics for personal and household items

### Authentication & Security
- **ASP.NET Identity**: Secure user registration and login
- **Role-Based Authorization**: Granular permissions for household management
- **Session Management**: Secure household context switching
- **Data Isolation**: Users only see their own and household items

## Technologies

### Backend
- .NET 10
- C# 13
- ASP.NET Core MVC
- Entity Framework Core 10
- ASP.NET Identity

### Frontend
- Razor Views (server-rendered)
- Bootstrap 5
- jQuery
- Chart.js 4.4

### Database
- Microsoft SQL Server 2019+
- EF Core Code-First Migrations

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server/sql-server-downloads) or SQL Server Express/LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (optional)
- [Node.js](https://nodejs.org/) (for Playwright tests)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Marwa-Sameer/CopilotCLIDemo.git
cd CopilotCLIDemo
```

### 2. Configure Database Connection

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HouseholdNeedsManager;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

For SQL Server Express, use:
```
Server=localhost\\SQLEXPRESS;Database=HouseholdNeedsManager;Trusted_Connection=True;TrustServerCertificate=True
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

This will create the database and seed the default roles (Owner, Admin, Member).

### 5. Run the Application

```bash
dotnet run
```

The application will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 6. Create Your Account

1. Navigate to the application URL
2. Click **Register** to create a new account
3. Fill in your email and password
4. Click **Register** to create your account

## Usage Guide

### Creating a Household

1. Click **Households** in the navigation menu
2. Click **Create New Household**
3. Enter a household name
4. Click **Create** - you'll become the Owner

### Adding Items

1. Click **My Items** in the navigation menu
2. Click **Create New Item**
3. Fill in the item details:
   - Choose **Personal** or **Household** item
   - Enter name, quantity, category, etc.
   - Mark as **Urgent** if needed
4. Click **Create**

### Managing Households

- **Switch Household**: Use the dropdown in the header
- **Join Household**: Go to Households → Join, enter household ID
- **Leave Household**: Click Leave on the household card (members only)
- **Delete Household**: Click Delete on the household card (owners only)

### Viewing Statistics

1. Click **Dashboard** in the navigation menu
2. View current week statistics
3. Scroll down for historical trends and charts

### Exporting Lists

1. Go to **My Items**
2. Apply any filters you want
3. Click **Export to CSV** to download
4. Click **Printable View** for a printer-friendly format

## Running Tests

### Playwright End-to-End Tests

```bash
cd tests/E2ETests
npm install
npx playwright install
npx playwright test
```

## Deployment

### Option 1: Deploy to IIS on Windows Server

See [docs/DEPLOYMENT_IIS.md](docs/DEPLOYMENT_IIS.md) for detailed instructions.

**Quick Steps:**
1. Publish: `dotnet publish -c Release`
2. Copy to IIS directory
3. Create Application Pool (.NET CLR: No Managed Code)
4. Create IIS Site
5. Configure SQL Server connection
6. Run migrations

### Option 2: Deploy with Docker Compose

See [docs/DEPLOYMENT_DOCKER.md](docs/DEPLOYMENT_DOCKER.md) for detailed instructions.

**Quick Steps:**
```bash
docker-compose up --build
```

Access at `http://localhost:8080`

## Project Structure

```
CopilotCLIDemo/
├── Controllers/          # MVC Controllers
│   ├── DashboardController.cs
│   ├── HouseholdsController.cs
│   ├── ItemsController.cs
│   └── HomeController.cs
├── Data/                 # Database context and migrations
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs
│   └── Migrations/
├── Models/               # Entity models
│   ├── ApplicationUser.cs
│   ├── Household.cs
│   ├── HouseholdMember.cs
│   ├── Category.cs
│   ├── Item.cs
│   └── HouseholdRole.cs
├── Services/             # Business logic services
│   └── HouseholdContextService.cs
├── Middleware/           # Custom middleware
│   └── HouseholdContextMiddleware.cs
├── ViewModels/           # View models
│   └── DashboardViewModel.cs
├── Views/                # Razor views
│   ├── Dashboard/
│   ├── Households/
│   ├── Items/
│   └── Shared/
├── wwwroot/              # Static files
│   ├── css/
│   ├── js/
│   └── lib/
├── tests/E2ETests/       # Playwright tests
├── docs/                 # Documentation
│   ├── DEPLOYMENT_IIS.md
│   └── DEPLOYMENT_DOCKER.md
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Architecture

### Data Model

- **ApplicationUser**: Extended Identity user with navigation properties
- **Household**: Contains items and members
- **HouseholdMember**: Join table for many-to-many relationship (User ↔ Household)
- **Category**: Household-specific categories for organizing items
- **Item**: Core entity representing a shopping list item

### Key Concepts

- **Personal Items**: Items with `HouseholdId == null` visible only to creator
- **Household Items**: Items with `HouseholdId != null` visible to all household members
- **Active Household**: Current household context stored in user session
- **Household Context Service**: Manages active household switching

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

Please ensure:
- Code follows C# coding conventions
- All tests pass
- New features include tests
- Documentation is updated

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions:
- Open an [Issue](https://github.com/Marwa-Sameer/CopilotCLIDemo/issues)
- Check existing [Discussions](https://github.com/Marwa-Sameer/CopilotCLIDemo/discussions)

## Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- UI powered by [Bootstrap](https://getbootstrap.com/)
- Charts by [Chart.js](https://www.chartjs.org/)
- Testing with [Playwright](https://playwright.dev/)

---

**Happy Shopping List Management! 🛒**