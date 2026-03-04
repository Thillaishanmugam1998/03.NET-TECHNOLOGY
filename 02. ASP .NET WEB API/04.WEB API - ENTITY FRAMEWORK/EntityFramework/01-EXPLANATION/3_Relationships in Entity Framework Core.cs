/*
================================================================================
ENTITY FRAMEWORK CORE – RELATIONSHIPS (BEGINNER TO ADVANCED)
================================================================================

This file explains:

1️⃣ Why do we need relationships in EF Core?
2️⃣ What exactly is a relationship in EF Core?
3️⃣ Different types of relationships
   - One-to-One
   - One-to-Many
   - Many-to-Many

Examples are kept simple and practical for freshers and experienced developers.

================================================================================
1️⃣ WHY DO WE NEED RELATIONSHIPS IN EF CORE?
================================================================================

Real databases are not just one table.
Data is connected.

Example:

Products belong to a Category.
Orders contain many Products.
Each Customer has many Orders.

Without relationships:

✔ Data will be duplicated
✔ Harder to maintain
✔ Updates can be inconsistent
✔ Queries become complex

Relationships provide:

✔ Data integrity
✔ Cleaner object models
✔ Easier queries using navigation properties
✔ Automatic foreign key handling

--------------------------------------------------------------------------------
2️⃣ WHAT EXACTLY IS A RELATIONSHIP IN EF CORE?
--------------------------------------------------------------------------------

A relationship is a connection between two entities (tables).

EF Core represents relationships using:

✔ Foreign keys (in the database)
✔ Navigation properties (in C# classes)

Example:

Product has CategoryId (foreign key)
Product has Category navigation property

Category has List<Product> navigation property

This allows:

context.Products.Include(p => p.Category)

================================================================================
3️⃣ DIFFERENT TYPES OF RELATIONSHIPS IN EF CORE
================================================================================

--------------------------------------------------------------------------------
3.1 ONE-TO-ONE
--------------------------------------------------------------------------------

One entity is related to exactly one other entity.

Example:

User ↔ UserProfile

One user has one profile.
One profile belongs to one user.

MODEL

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }

    public UserProfile Profile { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public string Address { get; set; }

    public int UserId { get; set; }       // Foreign Key
    public User User { get; set; }        // Navigation
}

EF Core can configure this automatically by conventions.

--------------------------------------------------------------------------------
3.2 ONE-TO-MANY
--------------------------------------------------------------------------------

One entity has many related entities.

Example:

Category → Products

One category has many products.
Each product belongs to one category.

MODEL

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Product> Products { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int CategoryId { get; set; }   // Foreign Key
    public Category Category { get; set; } // Navigation
}

Query example:

var categories = context.Categories
                        .Include(c => c.Products)
                        .ToList();

--------------------------------------------------------------------------------
3.3 MANY-TO-MANY
--------------------------------------------------------------------------------

Many entities on one side relate to many on the other.

Example:

Student ↔ Course

A student can join many courses.
A course can have many students.

EF Core uses a join table internally.

MODEL

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Course> Courses { get; set; }
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }

    public List<Student> Students { get; set; }
}

EF Core automatically creates a join table like:

StudentCourse
------------
StudentId | CourseId

Query example:

var students = context.Students
                      .Include(s => s.Courses)
                      .ToList();

================================================================================
SUMMARY
================================================================================

Why relationships?
-----------------
To connect data, reduce duplication, and simplify queries.

What is a relationship?
-----------------------
A link between two entities using foreign keys and navigation properties.

Types of relationships:
-----------------------
1. One-to-One
2. One-to-Many
3. Many-to-Many




================================================================================
END OF FILE
================================================================================
*/
