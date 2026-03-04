/*
================================================================================
ENTITY FRAMEWORK CORE – ENTITY STATES (BEGINNER TO ADVANCED)
================================================================================

This file explains:

1️⃣ What are Entity States in EF Core?
2️⃣ Why EF Core tracks entity state
3️⃣ The 5 main states with meaning
4️⃣ Real examples for CRUD
5️⃣ How EF Core decides the state
6️⃣ Manually setting state
7️⃣ Common mistakes and tips

Think of entity state as a status tag EF Core uses to decide what SQL to run.

================================================================================
1️⃣ WHAT ARE ENTITY STATES IN EF CORE?
================================================================================

Entity State = The current status of an entity inside DbContext.

EF Core uses this state to decide:

✔ Should it INSERT?
✔ Should it UPDATE?
✔ Should it DELETE?
✔ Should it do nothing?

States are stored in the Change Tracker.

--------------------------------------------------------------------------------
2️⃣ WHY EF CORE TRACKS ENTITY STATE
--------------------------------------------------------------------------------

EF Core needs to know:

✔ Is this object new?
✔ Was it changed?
✔ Is it marked for deletion?

It uses this information when you call:

    context.SaveChanges();

SaveChanges translates entity states to SQL commands.

--------------------------------------------------------------------------------
3️⃣ FIVE MAIN ENTITY STATES
--------------------------------------------------------------------------------

EntityState values:

1. Added      → New entity, will be INSERTED
2. Unchanged  → Exists in DB, no changes
3. Modified   → Exists in DB, will be UPDATED
4. Deleted    → Exists in DB, will be DELETED
5. Detached   → Not tracked by DbContext

================================================================================
4️⃣ REAL EXAMPLES WITH CRUD
================================================================================

Example entity:

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

----------------------------------------
CREATE (Added)
----------------------------------------

var product = new Product
{
    Name = "Laptop",
    Price = 95000
};

context.Products.Add(product);

// Entity State now: Added
// SaveChanges() will run INSERT

----------------------------------------
READ (Unchanged)
----------------------------------------

var product = context.Products.First(p => p.Id == 1);

// Entity State now: Unchanged
// SaveChanges() will do nothing

----------------------------------------
UPDATE (Modified)
----------------------------------------

var product = context.Products.First(p => p.Id == 1);
product.Price = 105000;

// Entity State now: Modified
// SaveChanges() will run UPDATE

----------------------------------------
DELETE (Deleted)
----------------------------------------

var product = context.Products.First(p => p.Id == 1);
context.Products.Remove(product);

// Entity State now: Deleted
// SaveChanges() will run DELETE

================================================================================
5️⃣ HOW EF CORE DECIDES THE STATE
================================================================================

Case 1: New object + Add()

    context.Add(new Product());
    → Added

Case 2: Fetched from DB

    context.Products.First();
    → Unchanged

Case 3: Change a property

    product.Price = 2000;
    → Modified

Case 4: Remove()

    context.Remove(product);
    → Deleted

Case 5: Not attached to DbContext

    var p = new Product { Id = 5 };
    → Detached

================================================================================
6️⃣ CHECK ENTITY STATE IN CODE
================================================================================

using Microsoft.EntityFrameworkCore;

var product = context.Products.First();

var entry = context.Entry(product);

Console.WriteLine(entry.State); // Unchanged, Modified, etc.

================================================================================
7️⃣ MANUALLY SET ENTITY STATE
================================================================================

This is useful when entity is not tracked (Detached).

----------------------------------------
Example: Update without fetching from DB
----------------------------------------

var product = new Product { Id = 5, Price = 7000 };

context.Entry(product).State = EntityState.Modified;

// EF Core will generate UPDATE using only Id and Price

----------------------------------------
Example: Delete without fetching
----------------------------------------

var product = new Product { Id = 5 };

context.Entry(product).State = EntityState.Deleted;

// EF Core will generate DELETE where Id = 5

================================================================================
8️⃣ COMMON MISTAKES AND TIPS
================================================================================

Mistake 1
---------
Calling SaveChanges with Detached entities.

Fix:
Attach or set state manually.

Mistake 2
---------
Updating without tracking.

Fix:
Use context.Update(entity) or set EntityState.Modified.

Tip 1
-----
Use AsNoTracking() for read-only queries to avoid overhead.

Tip 2
-----
Use Entry().State to debug issues.

Tip 3
-----
Only set Modified for properties you really changed.

================================================================================
SUMMARY
================================================================================

Entity State tells EF Core what SQL action to run.

Added     → INSERT
Unchanged → No SQL
Modified  → UPDATE
Deleted   → DELETE
Detached  → Not tracked

Understanding entity states helps you:

✔ Write correct CRUD code
✔ Avoid tracking bugs
✔ Improve performance

================================================================================
END OF FILE
================================================================================
*/
