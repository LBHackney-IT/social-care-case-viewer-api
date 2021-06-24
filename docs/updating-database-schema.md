# Updating the database schema

We currently manage database changes manually as we don't have database migrations set up. Below are the steps to take for when you need to make changes to the PostgreSQL database schema.

1. In the `database` repository, update the [schema.sql](https://github.com/LBHackney-IT/social-care-case-viewer-api/blob/master/database/schema.sql) file
2. In `SocialCareCaseViewerAPI/Infrastructure` either update an existing class or create a new class for the schema changes
3. To test locally run:

```
make restart-db
```

4. To make changes to an environment, [connect to the PostgreSQL database](connecting-to-a-database.md#connecting-to-the-postgresql-rds-postgresql-database)
5. Backup the table you're going to apply changes to by running

```
create dbo.<TABLE_NAME> as dbo.<TABLE_NAME>_bak_yyyy_mm_dd
```

6. Run the SQL statements to apply your schema changes
