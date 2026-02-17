namespace ServiceLifetimeLearningNotes
{

    #region INTRODUCTION

    /*
        SERVICE LIFETIME = How long an object lives in memory

        ASP.NET Core provides 3 lifetimes:

        1. Singleton  → One instance for entire application
        2. Scoped     → One instance per HTTP request
        3. Transient  → New instance every time requested


        WHY this concept exists?

        Because NOT all services should behave same.

        Some data:
        ✔ Shared for all users
        ✔ Some per user
        ✔ Some per operation


        Using wrong lifetime causes:

        ❌ Data leak
        ❌ Memory waste
        ❌ Wrong results
        ❌ Performance problems

    */

    #endregion

    #region SINGLETON LIFETIME

    /*
        DEFINITION:

        Singleton = Only ONE instance created
        Entire application shares SAME instance


        CREATED:
        Once when first requested

        DESTROYED:
        When application stops



        REAL-TIME SHOPPING EXAMPLE:

        AppConfigService

        Contains:

            TaxRate = 18%
            DeliveryCharge rules



        WHY SINGLETON?


        Because:

        Tax rate same for:

        User A
        User B
        User C


        WRONG if Scoped or Transient:


        Imagine 10,000 users:

        Scoped:

            Creates 10,000 objects

        Transient:

            Creates 50,000 objects


        ALL contain SAME tax value


        RESULT:

        ❌ Memory waste
        ❌ No benefit


        CORRECT:

        Use ONE instance → Singleton



        REAL-WORLD ANALOGY:

        Government Tax Rule

        Government does NOT create tax rule per citizen

        ONE rule applies to all

    */

    #endregion

    #region SCOPED LIFETIME

    /*
        DEFINITION:

        Scoped = One instance per HTTP Request


        CREATED:

        Every new request


        DESTROYED:

        After request ends



        REAL-TIME SHOPPING EXAMPLE:

        CartService


        Cart belongs to USER


        Example:

        User A cart:

            iPhone


        User B cart:

            Shoes



        WHY SCOPED?


        Because each user must have separate cart




        VERY DANGEROUS PROBLEM IF SINGLETON:


        Imagine Singleton CartService:


        User A adds:

            iPhone


        User B opens cart


        User B sees:

            iPhone


        DATA LEAK!!!


        This is CRITICAL SECURITY BUG




        WHY NOT TRANSIENT?


        Because:


        Single request may need cart multiple times


        Example:

        Summary calculation

        Tax calculation


        Transient creates multiple instances

        Scoped keeps same instance inside request




        REAL-WORLD ANALOGY:

        Hotel Room

        Each customer gets separate room

        Customers do NOT share room

    */

    #endregion

    #region TRANSIENT LIFETIME

    /*
        DEFINITION:

        Transient = New instance EVERY time requested



        CREATED:

        Every use



        DESTROYED:

        Immediately after use




        REAL-TIME SHOPPING EXAMPLE:

        DiscountService


        This service only:

            Calculates discount


        Does NOT store data

        Does NOT store user info




        WHY TRANSIENT?


        Because:

        Every calculation independent




        WHY NOT SINGLETON?


        No need share instance

        No benefit




        WHY NOT SCOPED?


        No need keep instance entire request





        REAL-WORLD ANALOGY:

        Calculator


        Each person uses calculator

        Calculator does calculation

        No memory needed after use

    */

    #endregion

    #region WHY NOT USE SINGLE LIFETIME FOR ALL SERVICES


    /*
        VERY IMPORTANT QUESTION


        Why not make ALL services Singleton?


        Because causes DATA LEAK


        Example:

        CartService as Singleton


        User A adds item

        User B sees same item


        WRONG!!!





        Why not make ALL services Scoped?


        Because causes MEMORY WASTE


        Example:

        TaxService

        Same tax for everyone


        Creating per request unnecessary





        Why not make ALL services Transient?


        Because causes PERFORMANCE ISSUE


        Example:

        CartService needed multiple times

        Transient creates again and again

        Waste CPU and memory





        FINAL TRUTH:


        Each service has different purpose

        So each needs different lifetime


    */

    #endregion

    #region FINAL REALTIME SUMMARY TABLE


    /*
        SERVICE              LIFETIME      WHY

        AppConfigService    Singleton     Same config for all users

        CartService         Scoped        Each user needs separate cart

        CartSummaryService  Scoped        Works per request

        DiscountService     Transient     Only calculation



    */

    #endregion

    #region SUPER SIMPLE MEMORY TRICK


    /*
        REMEMBER FOREVER:


        Singleton

            One for entire app



        Scoped

            One per request



        Transient

            One per use



    */

    #endregion

}
