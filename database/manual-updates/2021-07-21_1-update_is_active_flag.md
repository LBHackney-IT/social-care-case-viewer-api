# Update is_active flag on one misaligned record

## The problem we're trying to solve

When is_active flag for a worker is null, the application crashes. There is currently one record that has this problem. This update aims to fix it.

## Justification for doing a manual update

The bug makes it so that the record for this worker is not visible (therefore not updatable) from the front end

## The plan

1. update the is_active field to true for the record


## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-791

## SQL statement(s)

```sql
-- update statement
update dbo.sccv_worker set is_active = true where id = 1009;
```

## Useful resources

N/A
