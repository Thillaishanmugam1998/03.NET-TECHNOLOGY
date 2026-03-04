/*
================================================================================
ENTITY FRAMEWORK CORE – COMPLETE BEGINNER TO ADVANCED GUIDE
================================================================================

This file explains:

1️⃣ What is Entity Framework Core
2️⃣ What is ORM
3️⃣ Why ORM is needed
4️⃣ EF Core Development Approaches
5️⃣ Why Database Provider Package is needed
6️⃣ How LINQ is converted to SQL
7️⃣ What is DbContext
8️⃣ How Connection String works
9️⃣ What is Migration
🔟 Update-Database Command

Think of this file like a mini textbook for EF Core.

================================================================================
1️⃣ WHAT IS ENTITY FRAMEWORK CORE?
================================================================================

Entity Framework Core (EF Core) is a Microsoft ORM framework used in .NET
applications to communicate with a database using C# objects instead of SQL.

Without EF Core:

    Write SQL manually
    Use SqlConnection
    Use SqlCommand
    Use SqlDataReader

Example:

    SELECT * FROM Products WHERE Price > 1000

With EF Core:

    context.Products.Where(p => p.Price > 1000)

EF Core automatically converts this LINQ query to SQL.

Benefits:

✔ Less SQL code
✔ Strongly typed
✔ Easier maintenance
✔ Faster development
✔ Works with many databases

Supported Databases:

SQL Server
PostgreSQL
MySQL
SQLite
Oracle
Cosmos DB

================================================================================
2️⃣ WHAT IS ORM?
================================================================================

ORM = Object Relational Mapper

It maps:

    Database Tables  →  C# Classes
    Table Rows       →  Objects
    Columns          →  Properties

Example:

DATABASE TABLE

Products
--------------------------------
Id | Name | Price | Stock


C# CLASS

public class Product
{
    public int Id {get;set;}
    public string Name {get;set;}
    public decimal Price {get;set;}
    public int Stock {get;set;}
}

EF Core maps this class to the database table automatically.

So when we write:

    var product = new Product();

EF Core knows it belongs to "Products" table.

================================================================================
3️⃣ WHY DO WE NEED ORM?
================================================================================

Before ORM developers had problems:

Problem 1
---------
Writing large SQL queries everywhere.

Problem 2
---------
Mapping data manually.

Example without ORM:

    reader["Name"]
    reader["Price"]

Problem 3
---------
Database change means changing SQL everywhere.

Problem 4
---------
Hard to maintain large applications.

ORM solves these problems.

Advantages of ORM:

✔ Object oriented code
✔ Less database code
✔ Easy maintenance
✔ Automatic mapping
✔ Cross database support

================================================================================
4️⃣ EF CORE DEVELOPMENT APPROACHES
================================================================================

EF Core supports 3 approaches.

1️⃣ Code First
2️⃣ Database First
3️⃣ Model First (less used in EF Core)

----------------------------------------
CODE FIRST
----------------------------------------

Developer writes C# classes first.

EF Core generates database.

Steps:

1. Create Models
2. Create DbContext
3. Create Migration
4. Update Database

Used in:

✔ New applications
✔ Microservices
✔ Domain Driven Design

----------------------------------------
DATABASE FIRST
----------------------------------------

Database already exists.

EF Core generates models from database.

Command:

Scaffold-DbContext

Used in:

✔ Legacy projects
✔ Existing database systems

================================================================================
5️⃣ WHY DO WE NEED DATABASE PROVIDER PACKAGE?
================================================================================

EF Core itself does NOT know how to talk to databases.

It needs a **Database Provider**.

Provider responsibilities:

✔ Translate LINQ → SQL
✔ Execute query
✔ Handle database connection

Example packages:

Microsoft.EntityFrameworkCore
    Core EF functionality

Microsoft.EntityFrameworkCore.SqlServer
    SQL Server database provider

Microsoft.EntityFrameworkCore.Tools
    Migration tools

--------------------------------------------------------------------------------
EXAMPLE
--------------------------------------------------------------------------------

LINQ Query:

var products = context.Products
                      .Where(p => p.Price > 1000)
                      .ToList();

EF Core sends this to the provider.

If using SQL Server provider:

Generated SQL:

SELECT [p].[Id], [p].[Name], [p].[Price], [p].[Stock]
FROM [Products] AS [p]
WHERE [p].[Price] > 1000;

If using PostgreSQL provider:

SELECT p."Id", p."Name", p."Price", p."Stock"
FROM "Products" AS p
WHERE p."Price" > 1000;

Notice:

SQL syntax changes depending on database.

Provider handles this translation.

================================================================================
6️⃣ PRODUCT ENTITY (MODEL CLASS)
================================================================================
*/

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }
}


/*
================================================================================
7️⃣ WHAT IS DbContext?
================================================================================

DbContext is the **main class responsible for database communication**.

Think of it as:

DATABASE SESSION / DATABASE MANAGER

Responsibilities:

✔ Database connection
✔ Query execution
✔ Change tracking
✔ Saving data
✔ Mapping models to tables

Example DbContext
================================================================================
*/

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    /*
    DbContextOptions contains:

    ✔ Database Provider
    ✔ Connection String
    ✔ Configuration settings
    */

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /*
    DbSet represents a database table.
    */

    public DbSet<Product> Products { get; set; }
}


/*
================================================================================
8️⃣ DATABASE CONNECTION STRING
================================================================================

Connection string tells EF Core:

✔ Which database to connect
✔ Server location
✔ Database name
✔ Authentication details

Example:

"Server=.;Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True"

Configured in Program.cs

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

appsettings.json

{
 "ConnectionStrings": {
   "DefaultConnection":
   "Server=.;Database=ShopDB;Trusted_Connection=True;"
 }
}

Flow:

Application → DbContext → Provider → Database

================================================================================
9️⃣ WHAT IS MIGRATION?
================================================================================

Migration tracks database schema changes.

Example:

You create a model:

public class Product
{
    public int Id
    public string Name
}

Later you add:

public decimal Price

Now database must also change.

Migration records this change.

Command:

Add-Migration AddPriceColumn

Generated migration example:

migrationBuilder.AddColumn<decimal>(
    name: "Price",
    table: "Products",
    nullable: false);

Migration acts like:

DATABASE VERSION CONTROL

Benefits:

✔ Track schema changes
✔ Safe database updates
✔ Team collaboration

================================================================================
🔟 WHAT IS Update-Database COMMAND?
================================================================================

After creating migration, database is not updated yet.

You must apply migration using:

Update-Database

What happens internally:

Step 1
------
EF Core reads migration files

Step 2
------
Converts migration to SQL

Step 3
------
Executes SQL in database

Example SQL executed:

ALTER TABLE Products
ADD Price decimal(18,2)

Database now matches the model.

================================================================================
EF CORE COMPLETE WORKFLOW
================================================================================

Step 1
Create Model

Step 2
Create DbContext

Step 3
Add Connection String

Step 4
Install Packages

    Microsoft.EntityFrameworkCore
    Microsoft.EntityFrameworkCore.SqlServer
    Microsoft.EntityFrameworkCore.Tools

Step 5
Create Migration

    Add-Migration InitialCreate

Step 6
Apply Migration

    Update-Database

Step 7
Use DbContext in code

================================================================================
REAL WORLD FLOW
================================================================================

Controller
     ↓
Service Layer
     ↓
DbContext
     ↓
EF Core
     ↓
Provider
     ↓
Database

================================================================================
SUMMARY
================================================================================

EF Core
-------
ORM framework for .NET

ORM
---
Maps database tables to C# classes

DbContext
---------
Manages database communication

DbSet
-----
Represents database tables

Provider
--------
Converts LINQ → SQL

Migration
---------
Tracks database schema changes

Update-Database
---------------
Applies migration to database

================================================================================
END OF FILE
================================================================================
*/