# TechMove 

A modern ASP.NET Core MVC application for managing technology relocation services, client contracts, and service requests.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp)

##  Features

- **Client Management** - Create and manage client information with regional support
- **Contract Management** - Handle contracts with PDF uploads and status tracking
- **Service Requests** - Track service requests with automatic USD to ZAR conversion
- **File Storage** - Upload signed agreements (PDF, max 10MB)
- **Real-time Currency Conversion** - Live exchange rates via ExchangeRate-API
- **Role-Based Access** - Admin and Manager roles with different permissions
- **Design Patterns** - Observer, Factory, and Strategy patterns implemented

## 🛠 Technologies

- ASP.NET Core MVC 9.0
- C# 13.0
- Entity Framework Core 9.0
- SQL Server
- ASP.NET Core Identity
- Bootstrap 5 & Bootstrap Icons
- jQuery

## 📦 Installation

1. Clone the repository:
 https://github.com/JaonIsaacs/TechMoveMvcFinal.git cd TechMoveMvcFinal

 
2. Update the connection string in `appsettings.json` if needed

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
dotnet run

5. Navigate to `https://localhost:7248`

##  Default Login Credentials

| Email | Password | Role |
|-------|----------|------|
| admin@techmove.com | Admin@123 | Admin |
| manager@techmove.com | Manager@123 | Manager |

##  Resources

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [ExchangeRate-API](https://www.exchangerate-api.com/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)

##  Author

**Jaon Isaacs** - [GitHub](https://github.com/JaonIsaacs)

##  License

This project is licensed under the MIT License.

---

