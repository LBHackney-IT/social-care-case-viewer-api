# Add start and end fields to the worker team table

## The problem we're trying to solve

We need to add start and end columns to the worker team table in order to be able to run correct reports.

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL statements below one by one

## Link to Jira ticket

[SCT-1736] (https://hackney.atlassian.net/browse/SCT-1765)

## SQL statement(s)

```sql
ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN START_DATE timestamp;

ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN END_DATE timestamp;
```
