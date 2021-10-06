# Alter table

## The problem we're trying to solve

Alter the table currently used to store case status options for a person to store dates for hadling different scenarios.

## Justification for doing a manual update

We don't have database migrations set up for the API.

## The plan

1. Run SQL statements to create the tables, and migrate subtypes in Staging
2. Run SQL statements to create the tables, and migrate subtypes in Production

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1261

## SQL statement(s)

```sql
alter table  IF EXISTS dbo.sccv_person_case_status_field_option ADD COLUMN created_date timestamp;

alter table  IF EXISTS dbo.sccv_person_case_status_field_option ADD COLUMN start_date timestamp;
```

## Useful resources

None
