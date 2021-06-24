# Connecting to a database

> ⚠️ **Warning**: Particularly if you're connecting to a production database,
> proceed with caution and look to pair with someone else.

Using the AWS console, you can connect via a bastion host to the:

- [PostgreSQL database](#connecting-to-the-postgresql-rds-postgresql-database)
- [MongoDB database](#connecting-to-the-mongodb-documentdb-database)

## Prerequisites

- Access to the relevant AWS account for the environment

## Environment information

| Environment | AWS account       | Systems Manager key | EC2 jumpbox                      |
|-------------|-------------------|---------------------|----------------------------------|
| Staging     | StagingAPIs       | staging             | RDS jump box-Platform APIs (new) |
| Production  | Mosaic-Production | mosaic-prod         | bastion-rds-jump-box-prod        |

## Connecting to the PostgreSQL (RDS PostgreSQL) database

1. Within relevant AWS account, go to **Systems Manager** → **Parameter Store**
2. Search for **/social-care-case-viewer-api/\<systems-manager-key\>/postgres**
3. In a new tab, go to **Session Manager** under **Node Management** in **Systems Manager**
4. Click on **Start session**
5. Select the appropriate EC2 jumpbox and click on **Start session**
6. Using the parameters from Parameter Store in your other tab, run the following command:

```
$ psql --host=<hostname> --port=<port> --username=<username> --dbname=social_care
```

This will prompt you to enter the password.

7. Paste the password and hit enter (NB: the characters won't show)

This will connect you to the PostgreSQL database of the service API.

### Useful commands

To view all tables in the `dbo` schema:

```
\dt dbo.*
```

To view structure of a table:

```
\d dbo.<table-name>
# E.g. \d dbo.dm_persons
```

## Connecting to the MongoDB (DocumentDB) database

1. Within relevant AWS account, go to **Systems Manager** → **Parameter Store**
2. Search for **/social-care-case-viewer-api/\<environment-key\>/docdb-conn-string**
3. Change directory into `tmp` by running:

```
$ cd /tmp
```

4. Check that `rds-ca-2019.pem` file exists in the directory by running:

```
$ ls
```

5. If the file doesn’t exist, then you’ll to download it by running:

```
$ wget https://s3.amazonaws.com/rds-downloads/rds-ca-2019-root.pem
```

6. Using the parameter from Parameter Store in your other tab, run the following command by extracting parts from the connection string - `mongodb://<USERNAME>:<PASSWORD>@<HOSTNAME>:<PORT>`

```
mongo --ssl --host <HOSTNAME>:<PORT> --sslCAFile rds-ca-2019-root.pem --username <USERNAME> --password
```

This will prompt you to enter the password.

7. Paste the password and hit enter

This will connect you to the MongoDB of the service API.

8. Connect to the `social_care_db` database by running:

```
use social_care_db;
```

#### Useful commands

To verify that data exists for a collection:

```
db.<COLLECTION_NAME>.count();
# E.g. db.form_data.count();
# E.g. db['resident-case-submissions'].count();
```

See [MongoDB documentation on mongo shell commands](https://docs.mongodb.com/manual/reference/method/) for more info.
