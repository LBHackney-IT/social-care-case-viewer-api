CREATE SCHEMA IF NOT EXISTS dbo;

--Obsolete as of 21/12/2020
-- create table dbo.ASC_ALLOCATIONS
-- (
    -- MOSAIC_ID                   bigint not null,
    -- LAST_NAME                   varchar(30) not null,
    -- FIRST_NAME                  varchar(30) not null,
    -- DATE_OF_BIRTH               timestamp,
    -- AGE                         int,
    -- PRIMARY_SUPPORT_REASON      varchar(100),
    -- ALLOCATED_TEAM              varchar(50),
    -- ALLOCATED_WORKER            varchar(90),
    -- ADDRESS                     varchar(255),
    -- POST_CODE                   varchar(10),
    -- UPRN                        bigint,
    -- LONG_TERM_SERVICE           varchar(4),
    -- SOCIAL_CARE_INVOLVEMENT     varchar(4),
    -- SHORT_TERM_SUPPORT          varchar(4),
    -- HOUSEHOLD_COMPOSITION       varchar(50),
    -- FULL_NAME                   varchar(62) not null
-- );

-- create table dbo.CFS_ALLOCATIONS
-- (
    -- MOSAIC_ID                   bigint not null,
    -- FULL_NAME                   varchar(62) not null,
    -- GROUP_ID                    bigint,
    -- ETHNICITY                   varchar(33),
    -- SUB_ETHNICITY               varchar(33),
    -- RELIGION                    varchar(30),
    -- GENDER                      varchar(1),
    -- DATE_OF_BIRTH               timestamp,
    -- SERVICE_USER_GROUP          varchar(30),
    -- SCHOOL_NAME                 varchar(255),
    -- SCHOOL_ADDRESS              varchar(255),
    -- GP_NAME                     varchar(62),
    -- GP_ADDRESS                  varchar(150),
    -- GP_SURGERY                  varchar(100),
    -- ALLOCATED_WORKER            varchar(90),
    -- WORKER_TYPE                 varchar(100),
    -- ALLOCATED_WORKER_TEAM       varchar(50),
    -- TEAM_NAME                   varchar(50),
    -- ALLOCATION_START_DATE       timestamp,
    -- ALLOCATION_END_DATE         timestamp,
    -- LEGAL_STATUS                varchar(255),
    -- PLACEMENT                   varchar(255),
    -- ON_CP_REGISTER              varchar(3),
    -- CONTACT_ADDRESS             varchar(255),
    -- CASE_STATUS_OPEN_CLOSED     varchar(7),
    -- CLOSURE_DATE_IF_CLOSED      timestamp,
    -- LAST_NAME                   varchar(30) not null,
    -- FIRST_NAME                  varchar(30) not null,
    -- WORKER_EMAIL                varchar(62)
-- );

create table dbo.DM_PERSONS
(
    PERSON_ID             bigint GENERATED BY DEFAULT AS IDENTITY not null
        constraint XPKDM_PERSONS
            primary key,
    SSDA903_ID            varchar(10),
    NHS_ID                numeric(10),
    SCN_ID                numeric(9),
    UPN_ID                varchar(13),
    FORMER_UPN_ID         varchar(13),
    FULL_NAME             varchar(62) not null,
    TITLE                 varchar(8),
    FIRST_NAME            varchar(30),
    LAST_NAME             varchar(30),
    DATE_OF_BIRTH         timestamp,
    DATE_OF_DEATH         timestamp,
    GENDER                varchar(1),
    RESTRICTED            varchar(1),
    PERSON_ID_LEGACY      varchar(16),
    FULL_ETHNICITY_CODE   varchar(33),
    COUNTRY_OF_BIRTH_CODE varchar(16),
    IS_CHILD_LEGACY       varchar(1),
    IS_ADULT_LEGACY       varchar(1),
    NATIONALITY           varchar(80),
    RELIGION              varchar(80),
    MARITAL_STATUS        varchar(80),
    FIRST_LANGUAGE        varchar(100),
    FLUENCY_IN_ENGLISH    varchar(100),
    EMAIL_ADDRESS         varchar(240),
    CONTEXT_FLAG          varchar(1),
    SCRA_ID               varchar(13),
    INTERPRETER_REQUIRED  varchar(1),
    FROM_DM_PERSON        varchar(1)
);

create index xif1dm_persons
    on dbo.DM_PERSONS (FULL_ETHNICITY_CODE);

create unique index xif2dm_persons
    on dbo.DM_PERSONS (PERSON_ID_LEGACY);

create table dbo.DM_ADDRESSES
(
    REF_ADDRESSES_PEOPLE_ID integer GENERATED BY DEFAULT AS IDENTITY not null
        constraint XPKDM_ADDRESSES
            primary key,
    REF_ADDRESS_ID          numeric(9) not null,
    PERSON_ID               numeric(16),
    START_DATE              timestamp,
    END_DATE                timestamp,
    ADDRESS                 varchar(464),
    POST_CODE               varchar(16),
    DISTRICT                varchar(80),
    PAF_AUTHORITY           varchar(80),
    WARD                    varchar(80),
    IS_IN_LA_AREA           varchar(1),
    ADDRESS_TYPE            varchar(16),
    IS_CONTACT_ADDRESS      varchar(1),
    IS_DISPLAY_ADDRESS      varchar(1),
    HOUSING_TENURE          varchar(16),
    IS_HMO                  varchar(1),
    OZ_LGA_NAME             varchar(80),
    OZ_LGA_NUMBER           numeric(9),
    OZ_SLA_ID               numeric(9),
    OZ_LHD_ID               numeric(9),
    OZ_HACC_ID              numeric(9),
    OZ_PHAMS_ID             numeric(9),
    ACCESS_NOTES            varchar(2000),
    EASTING                 numeric(10, 2),
    NORTHING                numeric(10, 2),
    UNIQUE_ID               numeric(15),
    FROM_DM_PERSON          varchar(1)
);

-- sequence used to generate values for REF_ADDRESS_ID column and attach to column. start at 510000 is arbitrary.
CREATE SEQUENCE dm_addresses_ref_address_id_seq START 510000 OWNED BY DM_ADDRESSES.REF_ADDRESS_ID;

--support for data import
create table dbo.SCCV_PERSONS_IMPORT
(
    PERSON_ID             varchar(100) not null constraint XPKDM_PERSONS_IMPORT primary key,
    SSDA903_ID            varchar(10),
    NHS_ID                numeric(10),
    SCN_ID                numeric(9),
    UPN_ID                varchar(13),
    FORMER_UPN_ID         varchar(13),
    FULL_NAME             varchar(255) not null,
    TITLE                 varchar(8),
    FIRST_NAME            varchar(100),
    LAST_NAME             varchar(100),
    DATE_OF_BIRTH         timestamp,
    DATE_OF_DEATH         timestamp,
    GENDER                varchar(1),
    RESTRICTED            varchar(1),
    PERSON_ID_LEGACY      varchar(16),
    FULL_ETHNICITY_CODE   varchar(33),
    COUNTRY_OF_BIRTH_CODE varchar(16),
    IS_CHILD_LEGACY       varchar(1),
    IS_ADULT_LEGACY       varchar(1),
    NATIONALITY           varchar(80),
    RELIGION              varchar(80),
    MARITAL_STATUS        varchar(80),
    FIRST_LANGUAGE        varchar(100),
    FLUENCY_IN_ENGLISH    varchar(100),
    EMAIL_ADDRESS         varchar(240),
    CONTEXT_FLAG          varchar(1),
    SCRA_ID               varchar(13),
    INTERPRETER_REQUIRED  varchar(1),
    FROM_DM_PERSON        varchar(1)
);

create table dbo.SCCV_PERSONS_LOOKUP
(
    PERSON_ID             varchar(100) not null constraint XPKDM_PERSONS_LOOKUP primary key,
    NC_ID            	  varchar(100) not null,
    CREATED_ON            timestamp not null default now(),
	CONSTRAINT unique_ids UNIQUE (NC_ID)
);
-- alter person table to support longer names
ALTER TABLE dbo.DM_PERSONS ALTER COLUMN FIRST_NAME TYPE varchar(100);
ALTER TABLE dbo.DM_PERSONS ALTER COLUMN LAST_NAME TYPE varchar(100);
ALTER TABLE dbo.DM_PERSONS ALTER COLUMN FULL_NAME TYPE varchar(255);

create table dbo.SCCV_ALLOCATIONS
(
    MOSAIC_ID                   varchar(100) not null,
    FULL_NAME                   varchar(255),
    GROUP_ID                    bigint,
    ETHNICITY                   varchar(33),
    SUB_ETHNICITY               varchar(33),
    RELIGION                    varchar(30),
    GENDER                      varchar(1),
    DATE_OF_BIRTH               timestamp,
    SERVICE_USER_GROUP          varchar(30),
    SCHOOL_NAME                 varchar(255),
    SCHOOL_ADDRESS              varchar(255),
    GP_NAME                     varchar(62),
    GP_ADDRESS                  varchar(150),
    GP_SURGERY                  varchar(100),
    ALLOCATED_WORKER            varchar(90),
    WORKER_TYPE                 varchar(100),
    ALLOCATED_WORKER_TEAM       varchar(50),
    TEAM_NAME                   varchar(50),
    ALLOCATION_START_DATE       timestamp,
    ALLOCATION_END_DATE         timestamp,
    LEGAL_STATUS                varchar(255),
    PLACEMENT                   varchar(255),
    ON_CP_REGISTER              varchar(3),
    CONTACT_ADDRESS             varchar(255),
    CASE_STATUS_OPEN_CLOSED     varchar(7),
    CLOSURE_DATE_IF_CLOSED      timestamp,
    LAST_NAME                   varchar(100),
    FIRST_NAME                  varchar(100),
    WORKER_EMAIL                varchar(62),
	LAC 						varchar(10)
);

alter table dbo.sccv_allocations alter column full_name drop not null;
--import new persons script
do
$$
        declare
                new_person record;
                new_person_mosaic_id bigint;
        begin
        --TODO: truncate table for full import
                for new_person
                in SELECT
                        imports.first_name,
                        imports.last_name,
                        imports.full_name,
                        imports.date_of_birth,
                        imports.person_id,
                        imports.gender,
                        imports.restricted,
                        imports.context_flag

        FROM
                dbo.SCCV_PERSONS_LOOKUP as lookup RIGHT OUTER JOIN
                dbo.SCCV_PERSONS_IMPORT as imports ON lookup.NC_ID = imports.PERSON_ID
        WHERE lookup.PERSON_ID is null AND LOWER(imports.PERSON_ID) like 'nc%'

        loop
                new_person_mosaic_id = null;

                insert into dbo.dm_persons(
                        first_name,
                        last_name,
                        full_name,
                        date_of_birth,
                        gender,
                        restricted,
                        context_flag,
                        from_dm_person
                )
                values (
                        new_person.first_name,
                        new_person.last_name,
                        new_person.full_name,
                        new_person.date_of_birth,
                        new_person.gender,
                        new_person.restricted,
                        new_person.context_flag,
                        'N'
                )
                returning person_id into new_person_mosaic_id;

                insert into dbo.sccv_persons_lookup(person_id, nc_id)
                        values (new_person_mosaic_id, new_person.person_id);

                raise notice '% - %', new_person.person_id, new_person_mosaic_id;
        end loop;
end;
$$

-- workers and team support
CREATE TABLE DBO.SCCV_WORKER
(
	ID			bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_SCCV_WORKER PRIMARY KEY,
	EMAIL		varchar(62) NOT NULL,
	FIRST_NAME	varchar(100) NOT NULL,
	LAST_NAME	varchar(100) NOT NULL,
	TEAM_ID		bigint,
	ROLE		varchar(200)
);

CREATE TABLE DBO.SCCV_TEAM
(
	ID			bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_SCCV_TEAM PRIMARY KEY,
	NAME		varchar(200) NOT NULL,
	CONTEXT		varchar(1) NOT NULL
);

