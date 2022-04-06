# Add new structure for ILDS teams

## The problem we're trying to solve

We need this to reflect ILDS' structure

## Justification for doing a manual update

To keep track of what's been done

## The plan

1. Run the SQL script

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1806

## SQL statement(s)

```sql
insert into dbo.sccv_team (name, context) values ('ILDS', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Nursing discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Social work discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Occupational therapy discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Psychology discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Psychiatry discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Dietetics discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Speech and language therapy discipline', 'A');
insert into dbo.sccv_team (name, context) values ('ILDS: Physiotherapy discipline', 'A');
```