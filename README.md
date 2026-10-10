# BankFlow – Banking REST API

BankFlow is a banking REST API developed using ASP.NET Core Web API (.NET 8), Entity Framework Core and Microsoft SQL Server.

The project demonstrates backend development, database integration, financial transaction processing and RESTful API design.

## Technologies
- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- Swagger / OpenAPI
- LINQ

## Features
- Create bank accounts
- Retrieve bank account details
- Deposit and withdraw funds
- Transfer money between bank accounts
- View transaction history
- Validate account balances and transaction amounts
- SQL Server database transactions
- Optimistic concurrency detection using RowVersion

## Project Architecture
- **BankFlow.API** – REST API controllers and configuration
- **BankFlow.Business** – Reserved for business logic
- **BankFlow.Data** – Database context and EF Core migrations
- **BankFlow.Models** – Entity models and request models

## Running the Project
1. Clone the repository.
2. Open `BankFlow.sln` in Visual Studio.
3. Configure a local SQL Server connection.
4. Apply Entity Framework Core migrations.
5. Run `BankFlow.API`.
6. Test the endpoints using Swagger.

## Project Status
Completed educational backend API prototype.

This project is intended for learning and portfolio demonstration, not real-world banking use.
