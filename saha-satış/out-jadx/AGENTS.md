# Project Rules

- **Database Migrations:** The user relies on Excel to import data, which is saved locally via Room database. If you change a Room Entity schema, you **MUST** provide a proper `Migration` class in `DatabaseProvider.kt` and add it using `.addMigrations()`. 
- **NEVER** rely on `.fallbackToDestructiveMigration()` to handle schema changes, as this wipes all user data and frustrates the user. 
- **Performance:** When displaying large lists (Products, Customers), always use `remember { derivedStateOf { ... } }` for filtering and sorting, and always provide a unique `key` to `items()` inside `LazyColumn` to ensure scrolling is smooth.
