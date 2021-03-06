# Delete two invalid allocation records

## The problem we're trying to solve

There are two records in dbo.sccv_allocations_combined table that have invalid person IDs against them.
Person records with those IDs do not exist, so these allocation records should be deleted.

Invalid allocation records:
id = 78213 (person id 334146100)
id = 79637 (person id 333770219)

## Justification for doing a manual update

Because these records are linked to non existent person IDs, they cannot be deleted using the app and have to be removed manually using SQL. 

## The plan

1. Backup allocations table
2. Run the SQL query to delete the invalid records

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-395

## SQL statement(s)

```sql
-- Replace <yyyy_mm_dd> with current date
CREATE TABLE dbo.sccv_allocations_combined_<yyyy_mm_dd> as table dbo.sccv_allocations_combined;
```

```sql
DELETE FROM dbo.sccv_allocations_combined where id in(78213,79637);
```
