CREATE SCHEMA IF NOT EXISTS dbo;

CREATE TABLE dbo.DM_PERSONS (
  PERSON_ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_PERSONS PRIMARY KEY,
  SSDA903_ID varchar(10),
  NHS_ID numeric(10),
  SCN_ID numeric(9),
  UPN_ID varchar(13),
  FORMER_UPN_ID varchar(13),
  FULL_NAME varchar(62) NOT NULL,
  TITLE varchar(8),
  FIRST_NAME varchar(30),
  LAST_NAME varchar(30),
  DATE_OF_BIRTH timestamp,
  DATE_OF_DEATH timestamp,
  GENDER varchar(1),
  RESTRICTED varchar(1),
  PERSON_ID_LEGACY varchar(16),
  FULL_ETHNICITY_CODE varchar(33),
  COUNTRY_OF_BIRTH_CODE varchar(16),
  IS_CHILD_LEGACY varchar(1),
  IS_ADULT_LEGACY varchar(1),
  NATIONALITY varchar(80),
  RELIGION varchar(80),
  MARITAL_STATUS varchar(80),
  FIRST_LANGUAGE varchar(100),
  FLUENCY_IN_ENGLISH varchar(100),
  EMAIL_ADDRESS varchar(240),
  CONTEXT_FLAG varchar(1),
  SCRA_ID varchar(13),
  INTERPRETER_REQUIRED varchar(1),
  FROM_DM_PERSON varchar(1)
);

CREATE INDEX xif1dm_persons ON dbo.DM_PERSONS (FULL_ETHNICITY_CODE);

CREATE UNIQUE INDEX xif2dm_persons ON dbo.DM_PERSONS (PERSON_ID_LEGACY);

-- alter person table to support longer names
ALTER TABLE dbo.DM_PERSONS
  ALTER COLUMN FIRST_NAME TYPE varchar(100);

ALTER TABLE dbo.DM_PERSONS
  ALTER COLUMN LAST_NAME TYPE varchar(100);

ALTER TABLE dbo.DM_PERSONS
  ALTER COLUMN FULL_NAME TYPE varchar(255);

--add additional columns required by the interim solution
ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_SEXUAL_ORIENTATION varchar(100);

ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_PREFERRED_METHOD_OF_CONTACT varchar(100);

-- for audit
ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.DM_PERSONS
  ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);

---- sequence used to generate values for REF_ADDRESS_ID. Start at 510000 is arbitrary.
CREATE SEQUENCE dbo.dm_addresses_ref_address_id_seq
  START 510000;

CREATE TABLE dbo.DM_ADDRESSES (
  REF_ADDRESSES_PEOPLE_ID integer GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_ADDRESSES PRIMARY KEY,
  REF_ADDRESS_ID numeric(9) NOT NULL DEFAULT NEXTVAL('dbo.dm_addresses_ref_address_id_seq'),
  PERSON_ID numeric(16),
  START_DATE timestamp,
  END_DATE timestamp,
  ADDRESS varchar(464),
  POST_CODE varchar(16),
  DISTRICT varchar(80),
  PAF_AUTHORITY varchar(80),
  WARD varchar(80),
  IS_IN_LA_AREA varchar(1),
  ADDRESS_TYPE varchar(16),
  IS_CONTACT_ADDRESS varchar(1),
  IS_DISPLAY_ADDRESS varchar(1),
  HOUSING_TENURE varchar(16),
  IS_HMO varchar(1),
  OZ_LGA_NAME varchar(80),
  OZ_LGA_NUMBER numeric(9),
  OZ_SLA_ID numeric(9),
  OZ_LHD_ID numeric(9),
  OZ_HACC_ID numeric(9),
  OZ_PHAMS_ID numeric(9),
  ACCESS_NOTES varchar(2000),
  EASTING numeric(10, 2),
  NORTHING numeric(10, 2),
  UNIQUE_ID numeric(15),
  FROM_DM_PERSON varchar(1)
);

ALTER TABLE dbo.DM_ADDRESSES
  ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.DM_ADDRESSES
  ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.DM_ADDRESSES
  ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.DM_ADDRESSES
  ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);

--support for data import
CREATE TABLE dbo.SCCV_PERSONS_IMPORT (
  PERSON_ID varchar(100) NOT NULL CONSTRAINT XPKDM_PERSONS_IMPORT PRIMARY KEY,
  SSDA903_ID varchar(10),
  NHS_ID numeric(10),
  SCN_ID numeric(9),
  UPN_ID varchar(13),
  FORMER_UPN_ID varchar(13),
  FULL_NAME varchar(255) NOT NULL,
  TITLE varchar(8),
  FIRST_NAME varchar(100),
  LAST_NAME varchar(100),
  DATE_OF_BIRTH timestamp,
  DATE_OF_DEATH timestamp,
  GENDER varchar(1),
  RESTRICTED varchar(1),
  PERSON_ID_LEGACY varchar(16),
  FULL_ETHNICITY_CODE varchar(33),
  COUNTRY_OF_BIRTH_CODE varchar(16),
  IS_CHILD_LEGACY varchar(1),
  IS_ADULT_LEGACY varchar(1),
  NATIONALITY varchar(80),
  RELIGION varchar(80),
  MARITAL_STATUS varchar(80),
  FIRST_LANGUAGE varchar(100),
  FLUENCY_IN_ENGLISH varchar(100),
  EMAIL_ADDRESS varchar(240),
  CONTEXT_FLAG varchar(1),
  SCRA_ID varchar(13),
  INTERPRETER_REQUIRED varchar(1),
  FROM_DM_PERSON varchar(1)
);

CREATE TABLE dbo.SCCV_PERSONS_LOOKUP (
  PERSON_ID varchar(100) NOT NULL CONSTRAINT XPKDM_PERSONS_LOOKUP PRIMARY KEY,
  NC_ID varchar(100) NOT NULL,
  CREATED_ON timestamp NOT NULL DEFAULT now(),
  CONSTRAINT unique_ids UNIQUE (NC_ID)
);

CREATE TABLE dbo.SCCV_ALLOCATIONS (
  MOSAIC_ID varchar(100) NOT NULL,
  FULL_NAME varchar(255) NOT NULL,
  GROUP_ID bigint,
  ETHNICITY varchar(33),
  SUB_ETHNICITY varchar(33),
  RELIGION varchar(30),
  GENDER varchar(1),
  DATE_OF_BIRTH timestamp,
  SERVICE_USER_GROUP varchar(30),
  SCHOOL_NAME varchar(255),
  SCHOOL_ADDRESS varchar(255),
  GP_NAME varchar(62),
  GP_ADDRESS varchar(150),
  GP_SURGERY varchar(100),
  ALLOCATED_WORKER varchar(90),
  WORKER_TYPE varchar(100),
  ALLOCATED_WORKER_TEAM varchar(50),
  TEAM_NAME varchar(50),
  ALLOCATION_START_DATE timestamp,
  ALLOCATION_END_DATE timestamp,
  LEGAL_STATUS varchar(255),
  PLACEMENT varchar(255),
  ON_CP_REGISTER varchar(3),
  CONTACT_ADDRESS varchar(255),
  CASE_STATUS_OPEN_CLOSED varchar(7),
  CLOSURE_DATE_IF_CLOSED timestamp,
  LAST_NAME varchar(100),
  FIRST_NAME varchar(100),
  WORKER_EMAIL varchar(62),
  LAC varchar(10)
);

-- new simplified allocations table. Person, worker and team details are now available in separate tables
CREATE TABLE dbo.SCCV_ALLOCATIONS_COMBINED (
  ID serial PRIMARY KEY,
  MOSAIC_ID bigint NOT NULL,
  WORKER_ID bigint,
  ALLOCATION_START_DATE timestamp,
  ALLOCATION_END_DATE timestamp,
  CASE_STATUS varchar(200),
  CLOSURE_DATE_IF_CLOSED timestamp
);

ALTER TABLE dbo.SCCV_ALLOCATIONS_COMBINED
  ADD COLUMN TEAM_ID bigint;

ALTER TABLE dbo.SCCV_ALLOCATIONS_COMBINED
  ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.SCCV_ALLOCATIONS_COMBINED
  ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.SCCV_ALLOCATIONS_COMBINED
  ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.SCCV_ALLOCATIONS_COMBINED
  ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);

-- workers and team support
CREATE TABLE DBO.SCCV_WORKER (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_SCCV_WORKER PRIMARY KEY,
  EMAIL varchar(62) NOT NULL,
  FIRST_NAME varchar(100) NOT NULL,
  LAST_NAME varchar(100) NOT NULL,
  TEAM_ID bigint
);

ALTER TABLE DBO.SCCV_WORKER
    DROP COLUMN TEAM_ID;

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN ROLE varchar(200);

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN CONTEXT_FLAG varchar(1);

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN CREATED_BY varchar(300);

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN DATE_START timestamp;

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN DATE_END timestamp;

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN LAST_MODIFIED_BY varchar(300);

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN CREATED_AT timestamp;

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN LAST_MODIFIED_AT timestamp;

ALTER TABLE DBO.SCCV_WORKER
    ADD COLUMN IS_ACTIVE boolean;

ALTER TABLE DBO.SCCV_WORKER
  ADD CONSTRAINT sccv_worker_unique_email UNIQUE (EMAIL);

CREATE TABLE DBO.SCCV_TEAM (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_SCCV_TEAM PRIMARY KEY,
  NAME varchar(200) NOT NULL,
  CONTEXT varchar(1) NOT NULL
);

CREATE TABLE DBO.SCCV_WORKERTEAM (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPK_SCCV_WORKERTEAM PRIMARY KEY,
  WORKER_ID bigint NOT NULL,
  TEAM_ID bigint NOT NULL
);

--phone numbers
CREATE TABLE dbo.DM_TELEPHONE_NUMBERS (
  TELEPHONE_NUMBER_ID integer GENERATED BY DEFAULT AS IDENTITY NOT NULL,
  PERSON_ID numeric(16) NOT NULL,
  TELEPHONE_NUMBER varchar(32),
  TELEPHONE_NUMBER_TYPE varchar(80) NOT NULL,
  CONSTRAINT XPKDM_TELEPHONE_NUMBERS PRIMARY KEY (TELEPHONE_NUMBER_ID, PERSON_ID)
);

ALTER TABLE dbo.dm_telephone_numbers
  ALTER COLUMN telephone_number_id RESTART SET START 500000 SET
  MINVALUE 500000;

ALTER TABLE dbo.dm_telephone_numbers
  ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.dm_telephone_numbers
  ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.dm_telephone_numbers
  ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.dm_telephone_numbers
  ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);

--other names
CREATE TABLE dbo.SCCV_PERSON_OTHER_NAME (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPKDM_SCCV_PERSON_OTHER_NAME PRIMARY KEY,
  PERSON_ID bigint,
  FIRST_NAME varchar(100),
  LAST_NAME varchar(100)
);

ALTER TABLE dbo.SCCV_PERSON_OTHER_NAME
  ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.SCCV_PERSON_OTHER_NAME
  ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.SCCV_PERSON_OTHER_NAME
  ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.SCCV_PERSON_OTHER_NAME
  ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);

--audit table
CREATE TABLE dbo.sccv_audit (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPK_SCCV_AUDIT PRIMARY KEY,
  TABLE_NAME varchar(200) NOT NULL,
  ENTITY_STATE varchar(50) NOT NULL,
  DATE_TIME timestamp NOT NULL,
  KEY_VALUES json,
  OLD_VALUES json,
  NEW_values json
);

--worker import
CREATE TABLE dbo.sccv_worker_import (
  AllocatedWorker varchar(300),
  FirstName varchar(300),
  Surname varchar(300),
  Email varchar(300),
  WorkerType varchar(300),
  team_name varchar(300)
);

--indexes to improve performance
CREATE INDEX dm_addresses_person_id_idx ON dbo.dm_addresses (person_id);

CREATE INDEX dm_telephone_numbers_person_id_idx ON dbo.dm_telephone_numbers (person_id);

--for restricted CFS cases management (manual updates to get backup restore data in sync)
CREATE TABLE dbo.sccv_cfs_restricted_flag_import (
  person_id varchar(20),
  restricted varchar(10),
  mosaic_id bigint
);

--warning notes
CREATE TABLE dbo.sccv_warning_note (
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPK_SCCV_WARNING_NOTE PRIMARY KEY,
  PERSON_ID bigint NOT NULL,
  START_DATE timestamp,
  REVIEW_DATE timestamp,
  END_DATE timestamp,
  LAST_REVIEW_DATE TIMESTAMP,
  NEXT_REVIEW_DATE TIMESTAMP,
  INDIVIDUAL_NOTIFIED boolean,
  NOTIFICATION_DETAILS varchar(1000),
  REVIEW_DETAILS varchar(1000),
  NOTE_TYPE varchar(50),
  STATUS varchar(50),
  DATE_INFORMED timestamp,
  HOW_INFORMED varchar(50),
  WARNING_NARRATIVE varchar(1000),
  MANAGERS_NAME varchar(100),
  DATE_MANAGER_INFORMED timestamp,
  SCCV_CREATED_AT timestamp,
  SCCV_CREATED_BY varchar(300),
  SCCV_LAST_MODIFIED_AT timestamp,
  SCCV_LAST_MODIFIED_BY varchar(300)
);

--warning notes review
CREATE TABLE dbo.sccv_warning_note_review(
  ID bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL CONSTRAINT XPK_SCCV_WARNING_NOTE_REVIEW PRIMARY KEY,
  Warning_Note_Id bigint NOT NULL,
  REVIEW_DATE TIMESTAMP,
  INDIVIDUAL_NOTIFIED boolean,
  NOTES varchar(1000),
  MANAGERS_NAME varchar(100),
  DATE_MANAGER_INFORMED timestamp,
  SCCV_CREATED_AT timestamp,
  SCCV_CREATED_BY varchar(300),
  SCCV_LAST_MODIFIED_AT timestamp,
  SCCV_LAST_MODIFIED_BY varchar(300)
);
