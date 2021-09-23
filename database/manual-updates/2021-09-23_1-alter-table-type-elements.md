# Alter table

## The problem we're trying to solve

Alter the table currently used to store case status fields in order to store the element type and where in the lifecycle it appears.

## Justification for doing a manual update

We don't have database migrations set up for the API.

## The plan

1. Run SQL statements to create the tables, and migrate subtypes in Staging
2. Run SQL statements to create the tables, and migrate subtypes in Production

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1262

## SQL statement(s)

```sql
alter table  IF EXISTS dbo.sccv_case_status_field ADD COLUMN element_type varchar(300);

alter table  IF EXISTS dbo.sccv_case_status_field ADD COLUMN phase varchar(300);
```
