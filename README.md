# Social Care Case Viewer API

The Social Care Service API provides the backend service for the [Social Care Front end](https://github.com/LBHackney-IT/lbh-social-care)

It is a part of the Social Care system (see [Social Care System Architecture](https://github.com/LBHackney-IT/social-care-architecture/tree/main) for more details).

- [Social Care Case Viewer API](#social-care-case-viewer-api)
  - [Documentation](#documentation)
    - [C4 Component Diagram](#c4-component-diagram)
    - [Swagger API](#swagger-api)
  - [Getting started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Dockerised dependencies](#dockerised-dependencies)
    - [Installation](#installation)
  - [Contributing](#contributing)
  - [Common Processes](#common-processes)
    - [Running the application](#running-the-application)
    - [Running the tests](#running-the-tests)
      - [Using the terminal](#using-the-terminal)
      - [Using an IDE](#using-an-ide)
    - [Updating the Schema](#updating-the-schema)
  - [Active Contributors](#active-contributors)
  - [License](#license)

## Documentation

Higher level Architecture diagrams can be found in the  [Social Care System Architecture](https://github.com/LBHackney-IT/social-care-architecture/) repository.

The process and tooling for diagram creation is found [here](https://github.com/LBHackney-IT/social-care-architecture/process.md).

### C4 Component Diagram

![C4 Component Diagram](docs/component-diagram.svg)

### Swagger API

- [Staging Environment](https://dr03nduqxh.execute-api.eu-west-2.amazonaws.com/staging/swagger/index.html)

## Getting started

### Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop)
- [.NET Core 3.1](https://dotnet.microsoft.com/download)

### Dockerised dependencies

- PostgreSQL 12
- MongoDB

### Installation

1. Clone this repository

```sh
$ git clone git@github.com:LBHackney-IT/social-care-case-viewer-api.git
```

## Contributing

- `master` branch is responsible for the code running in production
- Changes to the codebase are first merged into the `development` branch to be verified for correctness in staging
- `development` branch is responsible for staging. When code is merged to the `staging` branch, the staging environment is automatically rebuilt and deployed.
- When changes are verified in staging, changes can be merged into `master`

## Common Processes

### Running the application

To serve the API locally, use:

```sh
$ cd SocialCareCaseViewerApi && dotnet run
```

The application will be served at http://localhost:5000.

### Running the tests

There are two ways of running the tests: using the terminal and using an IDE.

#### Using the terminal

To run all tests, use:

```sh
$ make test
```

To run some tests i.e. single or a group, run the test databases in the background:

```sh
$ make start-test-dbs
```

And then you can filter through tests, using the `--filter` argument of the
`dotnet test` command:

```sh
# E.g. for a specific test, use the test method name
$ dotnet test --filter GivenHttpClientReturnsValidResponseThenGatewayReturnsListCaseNotesResponse
# E.g. for a file, use the test class name
$ dotnet test --filter SocialCarePlatformAPIGatewayTests
```

If your docker test database is out of sync with the schema on your current banch run

```sh
$ make restart-db
```

See [Microsoft's documentation on running selective unit tests](https://docs.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest) for more information.

#### Using an IDE

Run the test databases in the background, using:

```sh
$ make start-test-dbs
```

This will allow you to run the tests as normal in your IDE.

### Updating the Schema

- In the database repository update the [schema.sql](https://github.com/LBHackney-IT/social-care-case-viewer-api/blob/master/database/schema.sql) file
- In SocialCareCaseViewerAPI/Infrastructure either update an existing class or create a new class for the schema changes
- To test locally run `make restart-db`
- To deploy changes to AWS:
    - Go to AWS account (staging or prod)
    - Go to Systems Manager
    - Go to Session Manager
    - Choose `RDS jump box-Platform APIs (new)` and click `Start Session`, this allows us to have a CLI into the instance hosting our database
    - Connect to PostgreSQL `psql --host=<hostname> --port=5600 --username=<username> --password=<password> --dbname=social_care`
    - Backup the table you are going to apply changes to `create table_backup as table_copied`, it can be useful use the same table name for the backup but to append the date to the table name
    - If we later make breaking changes to the table in use we rename the backup table to make it our `active` version of the table
    - Manually apply schema changes to the table we are interested in

## Active Contributors

- **Tuomo Karki**, Lead Developer at Hackney (tuomo.karki@hackney.gov.uk)
- **Ben Reynolds-Carr**, Junior Developer at Hackney (ben.reynolds-carr@hackney.gov.uk)
- **Jerome Wanliss**, Intern Software Engineer at Hackney (jerome.wanliss@hackney.gov.uk)
- **John Farrell**, Senior Software Engineer at Made Tech (john.farrell@hackney.gov.uk)
- **Renny Fadoju**, Software Engineer at Made Tech (renny.fadoju@hackney.gov.uk)
- **Neil Kidd**, Lead Software Engineer at Made Tech (neil.kidd@hackney.gov.uk)
- **Wen Ting Wang**, Software Engineer at Made Tech (wenting.wang@hackney.gov.uk)

## License

[MIT License](LICENSE)
