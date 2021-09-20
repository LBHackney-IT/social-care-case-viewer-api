# Adding data for Looked After Child

## The problem we're trying to solve

Populating the various Case Status tables for LAC

## Justification for doing a manual update

We don't have database migrations set up for the API.

## The plan

1. Run SQL statements to create the tables in Staging
2. Run SQL statements to create the tables in Production

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1233

## SQL statement(s)

```sql

INSERT INTO dbo.sccv_case_status_type(name, description) VALUES ('LAC', 'Looked After Child');

INSERT INTO dbo.sccv_case_status_field(fk_case_status_type_id, name, description)
VALUES ((select id from dbo.sccv_case_status_type where name ilike 'LAC'), 'legalStatus',
 'What is the child legal status?');

INSERT INTO dbo.sccv_case_status_field(fk_case_status_type_id, name, description)
VALUES ((select id from dbo.sccv_case_status_type where name ilike 'LAC'), 'placementType',
 'What is the latest the placement type?');

INSERT INTO dbo.sccv_case_status_field(fk_case_status_type_id, name, description)
VALUES ((select id from dbo.sccv_case_status_type where name ilike 'LAC'), 'reasonCeased',
 'What is the reason for the episode ending?'); 

ALTER TABLE DBO.SCCV_CASE_STATUS_FIELD_OPTION ALTER COLUMN description TYPE varchar(512);

INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'A3', 'Placed for adoption with parental/guardian consent with current foster carer(s) (under Section 19 of the Adoption and Children Act 2002) or with a freeing order where parental/guardian consent has been given (under Section 18(1)(a) of the Adoption Act 1976)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'A4', 'Placed for adoption with parental/guardian consent not with current foster carer(s) (under Section 19 of the Adoption and Children Act 2002) or with a freeing order where parental/guardian consent has been given under Section 18(1)(a) of the Adoption Act 1976');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'A5', 'Placed for adoption with placement order with current foster carer(s) (under Section 21 of the Adoption and Children Act 2002) or with a freeing order where parental/guardian consent was dispensed with (under Section 18(1)(b) the Adoption Act 1976)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'A6', 'Placed for adoption with placement order not with current foster carer(s) (under Section 21 of the Adoption and Children Act 2002) or with a freeing order where parental/guardian consent was dispensed with (under Section 18(1)(b) of the Adoption Act 1976)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'H5', 'Semi-independent living accommodation not subject to children’s homes regulations');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'K1', 'Secure children’s homes');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'K2', 'Children’s Homes subject to Children’s Homes Regulations P1 Placed with own parent(s) or other person(s) with parental responsibility');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'P2', 'Independent living for example in a flat, lodgings, bedsit, bed and breakfast (B&B) or with friends, with or without formal support');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'P3', 'Residential employment');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'R1', 'Residential care home');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'R2', 'National Health Service (NHS)/health trust or other establishment providing medical or nursing care');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'R3', 'Family centre or mother and baby unit');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'R5', 'Young offender institution (YOI)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'S1', 'All residential schools, except where dual-registered as a school and children’s home');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'T0', 'All types of temporary move (see paragraphs above for further details)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'T1', 'Temporary periods in hospital');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'T2', 'Temporary absences of the child on holiday');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'T3', 'Temporary accommodation whilst normal foster carer(s) is/are on holiday');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'T4', 'Temporary accommodation of seven days or less, for any reason, not covered by codes T1 to T3');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U1', 'Foster placement with relative(s) or friend(s) – long term fostering');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U2', 'Fostering placement with relative(s) or friend(s) who is/are also an approved adopter(s) – fostering for adoption /concurrent planning');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U3', 'Fostering placement with relative(s) or friend(s) who is/are not longterm or fostering for adoption /concurrent planning');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U4', 'Foster placement with other foster carer(s) – long term fostering');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U5', 'Foster placement with other foster carer(s) who is/are also an approved adopter(s) – fostering for adoption /concurrent planning');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'U6', 'Foster placement with other foster carer(s) – not long term or fostering for adoption /concurrent planning');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'placementType'), 'Z1', 'Other placements (must be listed on a schedule sent to DfE with annual submission)');

INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'C1',  'Interim care order');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'C2',  'Full care order');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'D1',  'Freeing order granted');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'E1',  'Placement order granted');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'V2',  'Single period of accommodation under section 20 (Children Act 1989)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'V3',  'Accommodated under an agreed series of short-term breaks, when individual episodes of care are recorded');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'V4',  'Accommodated under an agreed series of short-term breaks, when agreements are recorded (NOT individual episodes of care)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'L1',  'Under police protection and in local authority accommodation');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'L2',  'Emergency protection order (EPO)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'L3',  'Under child assessment order and in local authority accommodation');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'J1',  'Remanded to local authority accommodation or to youth detention accommodation');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'J2',  'Placed in local authority accommodation under the Police and Criminal Evidence Act 1984, including secure accommodation. However, this would not necessarily be accommodation where the child would be detained.');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'legalStatus'), 'J3',  'Sentenced to Youth Rehabilitation Order (Criminal Justice and Immigration Act 2008 as amended by Legal Aid, Sentencing and Punishment of Offenders Act (LASPOA) 2012 with residence or intensive fostering requirement)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'X1'  , 'Episode ceases, and new episode begins on same day, for any reason');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E11' , 'Adopted - application for an adoption order unopposed');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E12' , 'Adopted – consent dispensed with by the court');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E2'  , 'Died');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E3'  , 'Care taken over by another local authority in the UK');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E4A' , 'Returned home to live with parent(s), relative(s), or other person(s) with parental responsibility as part of the care planning process (not under a special guardianship order or residence order or (from 22 April 2014) a child arrangement order).');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E4B' , 'Returned home to live with parent(s), relative(s), or other person(s) with parental responsibility which was not part of the current care planning process (not under a special guardianship order or residence order or (from 22 April 2014) a child arrangement order).');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E13' , 'Left care to live with parent(s), relative(s), or other person(s) with no parental responsibility.');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E41' , 'Residence order (or, from 22 April 2014, a child arrangement order which sets out with whom the child is to live) granted');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E45' , 'Special guardianship order made to former foster carer(s), who was/are a relative(s) or friend(s)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E46' , 'Special guardianship order made to former foster carer(s), other than relative(s) or friend(s)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E47' , 'Special guardianship order made to carer(s), other than former foster carer(s), who was/are a relative(s) or friend(s)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E48' , 'Special guardianship order made to carer(s), other than former foster carer(s), other than relative(s) or friend(s)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E5'  , 'Moved into independent living arrangement and no longer looked-after: supportive accommodation providing formalised advice/support arrangements (such as most hostels, young men’s Christian association, foyers, staying close and care leavers projects). Includes both children leaving care before and at age 18');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E6'  , 'Moved into independent living arrangement and no longer looked-after : accommodation providing no formalised advice/support arrangements (such as bedsit, own flat, living with friend(s)). Includes both children leaving care before and at age 18');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E7'  , 'Transferred to residential care funded by adult social care services');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E9'  , 'Sentenced to custody');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E14' , 'Accommodation on remand ended');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E15' , 'Age assessment determined child is aged 18 or over and E5, E6 and E7 do not apply, such as an unaccompanied asylum-seeking child (UASC) whose age has been disputed');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E16' , 'Child moved abroad');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E17' , 'Aged 18 (or over) and remained with current carers (inc under staying put arrangements)');
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'reasonCeased'), 'E8'  , 'Period of being looked-after ceased for any other reason (where none of the other reasons apply)');