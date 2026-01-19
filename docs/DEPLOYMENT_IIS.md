# Deployment to IIS on Windows Server

This guide provides step-by-step instructions for deploying the Household Needs Manager application to IIS (Internet Information Services) on Windows Server.

## Prerequisites

- Windows Server 2016 or later
- IIS 10.0 or later installed
- .NET 10 Runtime (ASP.NET Core Runtime 10.0.x) installed
- SQL Server 2016 or later
- Administrator access to the server

## Step 1: Install Prerequisites

### 1.1 Install IIS
1. Open **Server Manager**
2. Click **Manage** → **Add Roles and Features**
3. Select **Web Server (IIS)**
4. Complete the installation wizard

### 1.2 Install .NET 10 Hosting Bundle
1. Download the [ASP.NET Core 10.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Run the installer
3. Restart IIS after installation:
   ```powershell
   net stop was /y
   net start w3svc
   ```

### 1.3 Install SQL Server
1. Download SQL Server 2019 or later
2. Install with default settings or custom configuration
3. Enable SQL Server Authentication (Mixed Mode)
4. Note the server name and credentials

## Step 2: Publish the Application

### 2.1 Publish from Development Machine
Run the following command in the project directory:

```bash
dotnet publish -c Release -o ./publish
```

This creates a `publish` folder with all necessary files.

### 2.2 Copy Files to Server
1. Copy the `publish` folder to the server (e.g., `C:\inetpub\HouseholdNeedsManager`)
2. Ensure the folder has proper permissions for IIS_IUSRS group

## Step 3: Configure SQL Server Database

### 3.1 Create Database
Connect to SQL Server and create a new database:

```sql
CREATE DATABASE HouseholdNeedsManager;
```

### 3.2 Configure Connection String
Edit `appsettings.Production.json` in the publish folder:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=HouseholdNeedsManager;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Replace:
- `YOUR_SERVER_NAME` with your SQL Server instance name
- `YOUR_USERNAME` with SQL Server username
- `YOUR_PASSWORD` with SQL Server password

## Step 4: Run EF Core Migrations

Navigate to the publish folder and run migrations:

```powershell
cd C:\inetpub\HouseholdNeedsManager
dotnet HouseholdNeedsManager.dll --environment Production
```

Or use the EF Core tools:

```powershell
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Navigate to project source directory
cd C:\path\to\source\HouseholdNeedsManager

# Run migration
dotnet ef database update --connection "YOUR_CONNECTION_STRING"
```

## Step 5: Configure IIS

### 5.1 Create Application Pool
1. Open **IIS Manager**
2. Right-click **Application Pools** → **Add Application Pool**
3. Configure:
   - **Name**: HouseholdNeedsManager
   - **.NET CLR version**: No Managed Code
   - **Managed pipeline mode**: Integrated
4. Click **OK**

### 5.2 Configure Application Pool Identity
1. Select the **HouseholdNeedsManager** app pool
2. Click **Advanced Settings**
3. Under **Process Model**, set **Identity** to **ApplicationPoolIdentity**
4. Click **OK**

### 5.3 Create IIS Website
1. Right-click **Sites** → **Add Website**
2. Configure:
   - **Site name**: HouseholdNeedsManager
   - **Application pool**: HouseholdNeedsManager
   - **Physical path**: `C:\inetpub\HouseholdNeedsManager`
   - **Binding**:
     - Type: http
     - IP address: All Unassigned
     - Port: 80
     - Host name: (your domain or leave blank)
3. Click **OK**

### 5.4 Set Folder Permissions
1. Right-click the `C:\inetpub\HouseholdNeedsManager` folder
2. Select **Properties** → **Security** tab
3. Click **Edit** → **Add**
4. Add **IIS_IUSRS** group
5. Grant **Read & Execute**, **List folder contents**, and **Read** permissions
6. Click **OK**

## Step 6: Configure HTTPS (Recommended)

### 6.1 Obtain SSL Certificate
- Purchase from a Certificate Authority (CA), or
- Use Let's Encrypt with [win-acme](https://www.win-acme.com/), or
- Use a self-signed certificate for testing

### 6.2 Add HTTPS Binding
1. In IIS Manager, select your site
2. Click **Bindings** → **Add**
3. Configure:
   - Type: https
   - IP address: All Unassigned
   - Port: 443
   - SSL certificate: Select your certificate
4. Click **OK**

### 6.3 Enforce HTTPS (Optional)
The application already has HTTPS redirection enabled in `Program.cs`.

## Step 7: Test the Application

1. Open a web browser
2. Navigate to `http://your-server-address` or `https://your-server-address`
3. You should see the Household Needs Manager homepage
4. Test registration, login, and creating items

## Troubleshooting

### Issue: 500.19 Error - Configuration Error
**Solution**: Ensure the .NET 10 Hosting Bundle is installed and IIS has been restarted.

### Issue: 500.30 Error - In-Process Start Failure
**Solution**: 
- Check the Application Event Log for detailed error messages
- Verify the application pool is set to "No Managed Code"
- Ensure all dependencies are in the publish folder

### Issue: Cannot connect to SQL Server
**Solution**:
- Verify SQL Server is running
- Check the connection string in `appsettings.Production.json`
- Ensure SQL Server allows remote connections
- Verify firewall rules allow SQL Server port (default 1433)
- Test connection with SQL Server Management Studio

### Issue: Database tables not created
**Solution**: 
- Run EF Core migrations manually using `dotnet ef database update`
- Check the connection string has proper permissions to create tables

### Issue: 403 Forbidden Error
**Solution**: 
- Verify IIS_IUSRS has read permissions on the application folder
- Check that the application pool identity has necessary permissions

## Monitoring and Maintenance

### View Logs
Application logs are written to:
- Windows Event Viewer → Application logs
- Custom log files (if configured) in the application folder

### Update Application
1. Stop the IIS site
2. Copy new published files to `C:\inetpub\HouseholdNeedsManager`
3. Run any new migrations
4. Start the IIS site

### Backup Database
Regularly back up the SQL Server database:

```sql
BACKUP DATABASE HouseholdNeedsManager
TO DISK = 'C:\Backups\HouseholdNeedsManager.bak'
WITH FORMAT, COMPRESSION;
```

## Security Best Practices

1. **Use HTTPS**: Always use SSL/TLS certificates in production
2. **Strong Passwords**: Use strong SQL Server passwords
3. **Firewall**: Configure Windows Firewall to allow only necessary ports
4. **Updates**: Keep Windows Server, IIS, and .NET runtime updated
5. **Least Privilege**: Use minimal permissions for application pool identity
6. **Connection Strings**: Never commit connection strings with passwords to source control

## Additional Resources

- [IIS Documentation](https://docs.microsoft.com/iis/)
- [ASP.NET Core Hosting on IIS](https://docs.microsoft.com/aspnet/core/host-and-deploy/iis/)
- [Entity Framework Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
