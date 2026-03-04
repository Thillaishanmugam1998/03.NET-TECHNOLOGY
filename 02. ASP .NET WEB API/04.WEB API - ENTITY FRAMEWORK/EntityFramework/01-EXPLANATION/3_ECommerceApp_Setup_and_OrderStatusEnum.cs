/*
================================================================================
3_ECOMMERCEAPP_SETUP_AND_ORDER_STATUS_ENUM
================================================================================

STEP 1: CREATE A NEW PROJECT USING VISUAL STUDIO 2022

1. Open Visual Studio 2022.
2. Click Create a new project.
3. Choose ASP.NET Core Web API and click Next.
4. Project name: ECommerceApp
5. Framework: .NET 8.0 (Long-term support)
6. Click Create.

--------------------------------------------------------------------------------
STEP 2: INSTALL REQUIRED EF CORE PACKAGES
--------------------------------------------------------------------------------

Open Package Manager Console:
Tools -> NuGet Package Manager -> Package Manager Console

Run:

Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools

Purpose of each package:

- Microsoft.EntityFrameworkCore
  Core EF ORM features.

- Microsoft.EntityFrameworkCore.SqlServer
  SQL Server database provider for EF Core.

- Microsoft.EntityFrameworkCore.Tools
  Enables commands like Add-Migration and Update-Database.

--------------------------------------------------------------------------------
STEP 3: CREATE THE FOLDER STRUCTURE
--------------------------------------------------------------------------------

Inside ECommerceApp, create these folders:

- Models
  Contains entity classes representing database tables and relationships.

- Data
  Holds AppDbContext and related data configuration (seeding/migrations setup).

- Enums
  Stores enums such as OrderStatusEnum for predefined constant values.

- DTOs
  Contains request/response transfer models used by API endpoints.

--------------------------------------------------------------------------------
STEP 4: CREATE ORDER STATUS ENUM
--------------------------------------------------------------------------------

Create file: Enums/OrderStatusEnum.cs

Use this code:

namespace ECommerceApp.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Returned = 7
    }
}

Why this enum is useful:

- Keeps order states type-safe and readable in code.
- Prevents invalid status values.
- Can be used while seeding an OrderStatus table.
- Improves domain clarity and database consistency.

Step 5: Create Entity Classes
We will create 9 Entities: BaseAuditableEntity, Customer, Profile, Address, Category, Product, OrderStatus, Order, and 
OrderItem. We will use Default Conventions only (no Data Annotations, no Fluent API). Relationships are automatically inferred from navigation properties.

Base Auditable Entity
Create a class file named BaseAuditableEntity.cs within the Models folder, 
then copy and paste the following code. 
This is a base class inherited by all entities to maintain common audit fields
such as creation and update timestamps, created by, updated by, and an IsActive flag for soft deletes. It ensures consistency and reusability of audit tracking across all tables without duplicating code.


Customer Entity
Create a class file named Customer.cs within the Models folder, then copy and paste the following code. The Customer class represents a registered user in the system. It serves as a Principal Entity for related data such as Profiles, Addresses, and Orders. It holds basic customer details such as name, email, and phone number, and maintains 1:1, 1:M, and M:M relationships through navigation properties.

Profile Entity
Create a class file named Profile.cs within the Models folder, then copy and paste the following code. The Profile class stores extended customer personal details, such as gender, display name, and date of birth. It has a one-to-one (1:1) relationship with the Customer, meaning each customer has exactly one profile, and its primary key is also the foreign key referencing the customer.

Address Entity
Create a class file named Address.cs within the Models folder, then copy and paste the following code. The Address class stores physical addresses used for billing or shipping. It has a one-to-many (1:M) relationship with the Customer; one customer can have multiple addresses, but each address belongs to a single customer. It includes address-related fields like street, city, postal code, and country.

Category Entity
Create a class file named Category.cs within the Models folder, then copy and paste the following code. The Category class represents a logical grouping of products, such as “Electronics” or “Books.” It is a Principal Entity in a one-to-many relationship with Product. Each category can have multiple products, enabling easier organization, filtering, and reporting of items by category.

Product Entity
Create a class file named Product.cs within the Models folder, then copy and paste the following code. The Product class represents individual items available for purchase. It belongs to one category and can appear in multiple orders through the OrderItem joining entity. It contains properties like name, price, stock, SKU, and description, making it central to the product catalog.

OrderStatus Entity
Create a class file named OrderStatus.cs within the Models folder, then copy and paste the following code. The OrderStatus class serves as a Master Lookup Table for storing predefined order states such as Pending, Confirmed, Shipped, Delivered, and Cancelled. It maintains a one-to-many relationship with the Order entity, ensuring consistent tracking of an order’s progress throughout its lifecycle.

Order Entity
Create a class file named Order.cs within the Models folder, then copy and paste the following code. The Order class represents a single placed order. It connects customers, addresses, and products through various relationships: 1:M with OrderItems, 1:M with Customer, and M:M via OrderItem with Product. It tracks order details, including the total amount, order date, and order status.

OrderItem Entity
Create a class file named OrderItem.cs within the Models folder, then copy and paste the following code. The OrderItem class is a Junction Entity between Orders and Products, enabling a many-to-many relationship. It stores item-level details, such as product ID, quantity, and unit price, for each product in an order.

Note: All navigations are virtual, so you can later enable lazy loading if you wish; for now, we will use eager loading in controller reads.

Step 6: Create the DbContext
Create a class file named AppDbContext.cs within the Data folder, 
then copy and paste the following code. The AppDbContext class is the EF Core bridge to the 
SQL Server database. It defines all DbSet properties for entities, manages relationships 
automatically through conventions, and seeds initial data for Customers, Products, Categories,
Orders, and Order Statuses using the HasData() method.


================================================================================
END
================================================================================
*/
