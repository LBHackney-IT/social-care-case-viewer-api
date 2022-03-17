# Additional fields for Allocation

## The problem we're trying to solve

We need to add RAG rating, Summary and CarePackage fields to the allocations table.
## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL query

## Link to Jira ticket

[SCT-1736] (https://hackney.atlassian.net/browse/SCT-1758)

## SQL statement(s)

```sql
alter table if exists dbo.SCCV_ALLOCATIONS_COMBINED
    add column RAG_RATING varchar,
    add column SUMMARY varchar,
    add column CARE_PACKAGE varchar
```
