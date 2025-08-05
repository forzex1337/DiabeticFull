# Diabetic Management Solution

A multi-project .NET solution for diabetic health management and monitoring.

## Projects

### 1. Diabetic (API Server)
- **Type**: ASP.NET Core Web API
- **Purpose**: Backend API for diabetic health data management
- **Features**: Glucose readings, meals, medications, and insulin tracking
- **Database**: PostgreSQL with Entity Framework Core
- **URL**: https://localhost:7030 (HTTPS) or http://localhost:5278 (HTTP)

### 2. Diabetic.Web (Web Application)
- **Type**: Blazor Server App
- **Purpose**: Web interface for managing diabetic health data
- **Features**: Glucose tracking, meal logging, medication management, and reports
- **Dependencies**: References Diabetic.Shared
- **URL**: https://localhost:7058 (HTTPS) or http://localhost:5294 (HTTP)

### 3. Diabetic.Shared (Shared Library)
- **Type**: .NET Class Library
- **Purpose**: Shared components, pages, and models
- **Features**: Razor components, Razor pages, data models
- **Shared Pages**: Home, Glucose, Meals, Medications, Insulin, Reports, Profile
- **Used by**: Diabetic.Web and Diabetic.Mobile

### 4. Diabetic.Mobile (Mobile App)
- **Type**: .NET MAUI Application
- **Purpose**: Cross-platform mobile app for diabetic management
- **Platforms**: Android, iOS, Windows, macOS
- **Dependencies**: References Diabetic.Shared

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL (for the API)
- Visual Studio 2022 or VS Code (recommended for mobile development)

### Quick Start

1. **Build all projects:**
   ```bash
   dotnet build
   ```

2. **Run individual projects:**
   - API Server: Double-click `run-api.bat`
   - Web App: Double-click `run-web.bat`
   - Mobile App: Double-click `run-mobile.bat`

3. **Run everything at once:**
   ```bash
   # Double-click run-all.bat or run:
   ./run-all.bat
   ```

### Manual Setup

1. **Database Setup** (for API):
   - Install PostgreSQL
   - Update connection string in `Diabetic/appsettings.json`
   - Run migrations: `dotnet ef database update` (from Diabetic directory)

2. **Run API Server:**
   ```bash
   cd Diabetic
   dotnet run
   ```

3. **Run Web Application:**
   ```bash
   cd Diabetic.Web
   dotnet run
   ```

4. **Build Mobile App:**
   ```bash
   cd Diabetic.Mobile
   dotnet build
   ```

## Development

### Solution Structure
```
Diabetic.sln                 # Solution file
├── Diabetic/                # API Server
├── Diabetic.Web/            # Web Application  
├── Diabetic.Shared/         # Shared Library
├── Diabetic.Mobile/         # Mobile App
├── run-*.bat               # Launch scripts
└── README.md               # This file
```

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build Diabetic/Diabetic.csproj
```

### Testing
```bash
# Run tests (if any)
dotnet test
```

## URLs & Ports

When running locally:
- **API**: https://localhost:7030 (swagger at /swagger)
- **Web App**: https://localhost:7058
- **Mobile**: Build and deploy to device/emulator

## Features

### Current
- ✅ Multi-project solution setup
- ✅ API with Entity Framework Core
- ✅ Web application with Blazor
- ✅ Mobile app with MAUI
- ✅ Shared component library
- ✅ **Shared pages between Web and Mobile**
- ✅ Automated build scripts
- ✅ Unified navigation across platforms

### Planned
- Database integration for web app
- Enhanced mobile UI
- Real-time updates
- Authentication system
- Advanced diabetic management features
- Integration with health devices
- Data export and sharing capabilities

## Troubleshooting

### Build Issues
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` if package restore fails
- Check that all project references are correct

### Runtime Issues
- For API: Check PostgreSQL connection and database setup
- For Web: Ensure API is running if web app depends on it
- For Mobile: Use Visual Studio for best mobile development experience

## Contributing

1. Build the solution: `dotnet build`
2. Make your changes
3. Test your changes
4. Create a pull request