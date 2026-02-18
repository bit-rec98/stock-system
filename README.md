# Stock System - Hardware Store Inventory Management

A full-stack inventory management system for hardware stores built with .NET 8 Web API and React TypeScript.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.3-3178C6?logo=typescript)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver)

## 🚀 Features

- **Authentication & Authorization**: JWT-based authentication with Admin-only access control
- **Product Management**: Full CRUD operations for inventory items
- **Categories & Suppliers**: Organize products by category and supplier
- **Advanced Filtering**: Search, filter by category/supplier, price range, stock status
- **Pagination**: Server-side pagination with customizable page sizes
- **Export Reports**: Generate PDF and Excel inventory reports with applied filters
- **Low Stock Alerts**: Dashboard highlights products below minimum stock levels
- **State Management**: Redux Toolkit with RTK Query for efficient data fetching
- **Responsive Design**: Material-UI based responsive interface

## 🏗️ Architecture

### Backend (.NET 8)
- **Clean Architecture** with 4 layers:
  - `Domain`: Entities, interfaces, business logic contracts
  - `Application`: DTOs, services interfaces, validators, mappings
  - `Infrastructure`: EF Core, repositories, service implementations
  - `API`: Controllers, authentication, Swagger documentation

### Design Patterns & Principles
- **Repository Pattern** with Unit of Work
- **SOLID Principles** throughout the codebase
- **Dependency Injection** for loose coupling
- **AutoMapper** for object-to-object mapping
- **FluentValidation** for request validation

### Frontend (React + TypeScript)
- **Vite** for fast development and building
- **Redux Toolkit** with RTK Query for state management
- **React Router v6** for navigation
- **Material-UI (MUI)** component library
- **React Hook Form** for form handling

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB works for development)

## 🛠️ Installation & Setup

### Backend Setup

1. Navigate to the backend folder:
   ```powershell
   cd backend
   ```

2. Restore NuGet packages:
   ```powershell
   dotnet restore
   ```

3. Update the connection string in `src/StockSystem.API/appsettings.json` if needed:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StockSystemDb;Trusted_Connection=True;"
     }
   }
   ```

4. Run the API (database will be created and seeded automatically):
   ```powershell
   dotnet run --project src/StockSystem.API
   ```

   The API will be available at: `https://localhost:7001` or `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend folder:
   ```powershell
   cd frontend
   ```

2. Install npm packages:
   ```powershell
   npm install
   ```

3. Start the development server:
   ```powershell
   npm run dev
   ```

   The app will be available at: `http://localhost:5173`

## 🔐 Default Credentials

The database is seeded with a default admin user:
- **Email**: `admin@stocksystem.com`
- **Password**: `Admin123!`

## 📁 Project Structure

```
stock-system/
├── backend/
│   ├── StockSystem.sln
│   └── src/
│       ├── StockSystem.Domain/          # Entities, Interfaces
│       ├── StockSystem.Application/     # DTOs, Services, Validators
│       ├── StockSystem.Infrastructure/  # EF Core, Repositories
│       └── StockSystem.API/             # Controllers, Configuration
│
└── frontend/
    ├── package.json
    ├── vite.config.ts
    └── src/
        ├── components/     # Reusable UI components
        ├── pages/          # Page components
        ├── store/          # Redux store, slices, API
        └── types/          # TypeScript interfaces
```

## 🔌 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Products (Requires Admin Role)
- `GET /api/products` - Get paginated products with filters
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/low-stock` - Get low stock products
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /api/products/export/pdf` - Export to PDF
- `GET /api/products/export/excel` - Export to Excel

### Categories (Requires Admin Role)
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Suppliers (Requires Admin Role)
- `GET /api/suppliers` - Get all suppliers
- `GET /api/suppliers/{id}` - Get supplier by ID
- `POST /api/suppliers` - Create supplier
- `PUT /api/suppliers/{id}` - Update supplier
- `DELETE /api/suppliers/{id}` - Delete supplier

## 📊 Sample Data

The database is seeded with sample data including:
- 8 product categories (Hand Tools, Power Tools, Plumbing, etc.)
- 3 suppliers
- 18 sample products with varying stock levels

## 🛡️ Security Features

- JWT Bearer token authentication
- Password hashing with ASP.NET Identity
- Role-based authorization (Admin only)
- CORS configuration for frontend
- Input validation with FluentValidation

## 🧪 Technologies Used

### Backend
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- ASP.NET Core Identity
- AutoMapper
- FluentValidation
- QuestPDF (PDF generation)
- ClosedXML (Excel export)
- Swagger/OpenAPI

### Frontend
- React 18
- TypeScript 5.3
- Vite 5
- Redux Toolkit + RTK Query
- Material-UI 5
- React Router 6
- React Hook Form
- Axios
- React Toastify

## 📝 License

This project is open source and available under the [MIT License](LICENSE).