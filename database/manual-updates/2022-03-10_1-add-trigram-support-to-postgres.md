# Add trigram support

## The problem we're trying to solve

We have updated the person search to use new fuzzy names matching that uses the postgres trigram extension in queries. This update installs all required extensions to add the required functionality and also creates a new index to improve performance.

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL queries below one by one

## Link to Jira ticket

[SCT-1736] (https://hackney.atlassian.net/browse/SCT-1736)

## SQL statement(s)

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS btree_gin;

CREATE INDEX CONCURRENTLY IF NOT EXISTS index_full_name_search
    ON dbo.dm_persons
        USING gin (first_name gin_trgm_ops, last_name gin_trgm_ops);
```
