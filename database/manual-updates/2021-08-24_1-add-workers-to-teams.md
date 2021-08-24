# Add workers to new teams

## The problem we're trying to solve

Some workers need to be added to a second team so they can allocate records correctly 

## Justification for doing a manual update

No feature in the app to support this

## The plan

<!-- Add your rough high-level steps for what you're going to do -->

1. Run the commands below

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1114

## SQL statement(s)

```sql
insert into dbo.sccv_workerteam(worker_id, team_id) values (1021,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1031,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (42,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1034,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1022,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1025,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1023,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1026,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1027,133);
```
