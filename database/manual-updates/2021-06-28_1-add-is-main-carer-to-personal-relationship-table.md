# Add `is_main_carer` to personal relationship table

## The problem we're trying to solve

We need to add an additional flag to the `dbo.sccv_personal_relationship` table.

## Justification for doing a manual update

We don't have database migrations set up.

## The plan

1. Run SQL statement on staging
2. Run SQL statement on production

## Link to Jira ticket

[<!-- Add the link to the Jira ticket -->](https://hackney.atlassian.net/browse/SCT-263)

## SQL statement(s)

```sql
ALTER TABLE dbo.sccv_personal_relationship
  ADD COLUMN is_main_carer varchar(1);
```

## Useful resources

N/A
