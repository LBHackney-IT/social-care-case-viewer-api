# Title

## The problem we're trying to solve

Some workers need to be added to a second team so they can allocate records correctly

## Justification for doing a manual update

No feature in the app to support this

## The plan

1. create a backup of the impacted table
2. insert new records

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1154

## SQL statement(s)

CREATE TABLE dbo.sccv_workerteam_2021_09_03 as table dbo.sccv_workerteam;

insert into dbo.sccv_workerteam(worker_id, team_id) values (1029,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1033,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1030,133);
insert into dbo.sccv_workerteam(worker_id, team_id) values (1032,133);

