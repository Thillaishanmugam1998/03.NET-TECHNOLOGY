# Employee Management API – Step-by-Step Guide

> **Stack:** ASP.NET Core Web API · .NET 10 · In-Memory Data · No Auth · No DTOs · No Interfaces

---

## 1. Project Structure

```
EmployeeAPI/
│
├── Controllers/
│   └── EmployeesController.cs   ← 5 CRUD endpoints
│
├── Models/
│   └── Employee.cs              ← Single model class
│
├── Services/
│   └── EmployeeService.cs       ← In-memory List + CRUD logic
│
├── Properties/
│   └── launchSettings.json      ← Local run profiles
│
├── appsettings.json             ← Base config
├── appsettings.Development.json ← Dev overrides
├── appsettings.Staging.json     ← Staging overrides
├── EmployeeAPI.csproj           ← Project file
├── Program.cs                   ← Entry point
└── steps.md                     ← This file
```

---

## 2. Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| Visual Studio 2022 or VS Code | Latest |

Check your version:
```bash
dotnet --version
```

---

## 3. Create & Run the Project

### Step 1 – Create the project

```bash
dotnet new webapi -n EmployeeAPI -f net10.0
cd EmployeeAPI
```

### Step 2 – Add Swagger package

```bash
dotnet add package Swashbuckle.AspNetCore
```

### Step 3 – Copy in the source files

Replace the generated files with the files from this project.

### Step 4 – Build

```bash
dotnet build
```

### Step 5 – Run

```bash
dotnet run
```

Open your browser at `https://localhost:7100` – Swagger loads automatically.

---

## 4. API Endpoints

| Method | URL | What it does |
|--------|-----|-------------|
| `GET` | `/api/employees` | Get all employees |
| `GET` | `/api/employees/{id}` | Get one employee |
| `POST` | `/api/employees` | Create new employee |
| `PUT` | `/api/employees/{id}` | Update employee |
| `DELETE` | `/api/employees/{id}` | Delete employee |

---

## 5. Sample JSON

### POST /api/employees – Create

```json
{
  "employeeName": "David Chen",
  "address": "99 Pine Street, Seattle",
  "mobileNumber": "+1-206-555-0404",
  "email": "david.chen@company.com",
  "dateOfBirth": "1992-07-20",
  "department": "Engineering",
  "salary": 110000,
  "yearsOfExperience": 9
}
```

### Response (201 Created)

```json
{
  "id": 4,
  "employeeName": "David Chen",
  "address": "99 Pine Street, Seattle",
  "mobileNumber": "+1-206-555-0404",
  "email": "david.chen@company.com",
  "dateOfBirth": "1992-07-20T00:00:00",
  "department": "Engineering",
  "salary": 110000,
  "yearsOfExperience": 9
}
```

### 404 – Not Found

```
"Employee with ID 99 not found."
```

---

## 6. How It Works

```
HTTP Request
    │
    ▼
Program.cs  →  Middleware (HTTPS → Controllers)
    │
    ▼
EmployeesController  (injects EmployeeService)
    │
    ▼
EmployeeService  (List<Employee> lives here)
    │
    ▼
JSON Response back to client
```

**Why Singleton?**
EmployeeService is registered as AddSingleton so the same instance (and its in-memory list) is shared across all requests. If it were AddScoped or AddTransient, the list would reset on every request.

---

## 7. Environments

| File | When loaded |
|------|-------------|
| `appsettings.json` | Always |
| `appsettings.Development.json` | ASPNETCORE_ENVIRONMENT=Development |
| `appsettings.Staging.json` | ASPNETCORE_ENVIRONMENT=Staging |

Set environment when running:
```bash
# macOS / Linux
ASPNETCORE_ENVIRONMENT=Staging dotnet run

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Staging"; dotnet run
```

---

## 8. Next Steps (When You're Ready)

- **Add a real database** – Install EF Core, replace EmployeeService with a DbContext-based version
- **Add validation** – Add [Required], [EmailAddress] attributes on the Employee model
- **Add pagination** – Accept ?page=1&size=10 query params in GetAll()
- **Add authentication** – Add JWT Bearer auth and [Authorize] on the controller

---

*ASP.NET Core Web API · .NET 10 · February 2026*
