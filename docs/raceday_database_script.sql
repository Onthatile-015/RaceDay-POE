CREATE DATABASE RaceDayDB;

USE RaceDayDB;

CREATE TABLE dbo.[User] (
    userId          INT IDENTITY(1,1) PRIMARY KEY,
    fullName        NVARCHAR(100)   NOT NULL,
    email           NVARCHAR(150)   NOT NULL UNIQUE,
    passwordHash    NVARCHAR(255)   NOT NULL,
    role            VARCHAR(20)     NOT NULL
                        CONSTRAINT CK_User_Role CHECK (role IN ('Organiser','Participant')),
    createdAt       DATETIME        NOT NULL DEFAULT GETDATE()
);

SELECT * FROM dbo.[User]

CREATE TABLE dbo.Event (
    eventId         INT IDENTITY(1,1) PRIMARY KEY,
    organiserId     INT             NOT NULL,
    name            NVARCHAR(150)   NOT NULL,
    description     NVARCHAR(MAX)   NULL,
    eventDate       DATE            NOT NULL,
    distanceKm      DECIMAL(5,2)    NOT NULL,
    status          VARCHAR(20)     NOT NULL DEFAULT 'Upcoming',
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (organiserId)
        REFERENCES dbo.[User](userId)
);

CREATE TABLE dbo.Category (
    categoryId      INT IDENTITY(1,1) PRIMARY KEY,
    eventId         INT             NOT NULL,
    name            VARCHAR(50)     NOT NULL,
    maxParticipants INT             NOT NULL DEFAULT 100,
    entryFee        DECIMAL(8,2)    NOT NULL DEFAULT 0,
    CONSTRAINT FK_Category_Event FOREIGN KEY (eventId)
        REFERENCES dbo.Event(eventId)
);


CREATE TABLE dbo.Venue (
    venueId         INT IDENTITY(1,1) PRIMARY KEY,
    eventId         INT             NOT NULL,
    address         NVARCHAR(200)   NOT NULL,
    city            NVARCHAR(100)   NOT NULL,
    latitude        DECIMAL(9,6)    NULL,
    longitude       DECIMAL(9,6)    NULL,
    CONSTRAINT FK_Venue_Event FOREIGN KEY (eventId)
        REFERENCES dbo.Event(eventId)
);


CREATE TABLE dbo.Enrolment (
    enrolmentId     INT IDENTITY(1,1) PRIMARY KEY,
    participantId   INT             NOT NULL,
    categoryId      INT             NOT NULL,
    enrolmentDate   DATETIME        NOT NULL DEFAULT GETDATE(),
    status          VARCHAR(20)     NOT NULL DEFAULT 'Active',
    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (participantId)
        REFERENCES dbo.[User](userId),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (categoryId)
        REFERENCES dbo.Category(categoryId),
    CONSTRAINT UQ_Enrolment_ParticipantCategory UNIQUE (participantId, categoryId)
);


CREATE TABLE dbo.Result (
    resultId                INT IDENTITY(1,1) PRIMARY KEY,
    enrolmentId              INT            NOT NULL UNIQUE,
    finishTime                TIME           NULL,
    position                   INT            NULL,
    capturedByOrganiserId  INT            NOT NULL,
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (enrolmentId)
        REFERENCES dbo.Enrolment(enrolmentId),
    CONSTRAINT FK_Result_Organiser FOREIGN KEY (capturedByOrganiserId)
        REFERENCES dbo.[User](userId)
);


--Organisers
INSERT INTO dbo.[User] (fullName, email, passwordHash, role) VALUES
('Onthatile Letsatsi',   'onthatileontha@gmail.com',   'HASHED_PW_1', 'Organiser'),
('Matlhatsi Mogale',   'matlhatsi90@gmail.com',   'HASHED_PW_2', 'Organiser');
 
-- Participants 
INSERT INTO dbo.[User] (fullName, email, passwordHash, role) VALUES
('Sphiwe Mohlala',  'Sphiwe20@gmail.com',  'HASHED_PW_3', 'Participant'),
('Reitumetse Chiloane',   'Reitu45@gmail.com',   'HASHED_PW_4', 'Participant');
 
-- Events (3) - organiserId 1 and 2 refer to the Organisers inserted above
INSERT INTO dbo.Event (organiserId, name, description, eventDate, distanceKm, status) VALUES
(1, 'Joburg City Marathon',    'Annually road marathon through the Johannesburg CBD.', '2026-10-12', 42.20, 'Upcoming'),
(1, 'Soweto Fun Run',          'Community fun run supporting local charities.',      '2026-10-25', 10.00, 'Upcoming'),
(2, 'Durban Coastal Cycle Tour','Scenic cycling tour along the Durban coastline.',    '2026-11-15', 90.00, 'Upcoming');
 
-- Categories (at least one per event)
INSERT INTO dbo.Category (eventId, name, maxParticipants, entryFee) VALUES
(1, 'Full Marathon (42km)', 2000, 350.00),
(1, 'Half Marathon (21km)', 1500, 250.00),
(2, '10km Fun Run',          800, 100.00),
(2, '5km Family Walk',       500,  50.00),
(3, '90km Full Tour',       1000, 400.00),
(3, '45km Half Tour',        800, 250.00);
 
-- Venues
INSERT INTO dbo.Venue (eventId, address, city, latitude, longitude) VALUES
(1, 'Mary Fitzgerald Square, Newtown', 'Johannesburg', -26.201900, 28.033700),
(2, 'Walter Sisulu Square', 'Soweto',                     -26.267800, 27.858900),
(3, 'North Beach Promenade', 'Durban',                     -29.856700, 31.037800);
 
-- Sample Enrolments 
INSERT INTO dbo.Enrolment (participantId, categoryId, status) VALUES
(3, 1, 'Active'),  -- Sphiwe > Full Marathon
(3, 5, 'Active'),  -- Sphiwe > 90km Full Tour
(4, 2, 'Active'),  -- Reitumetse > Half Marathon
(4, 3, 'Active');  -- Reitumetse > 10km Fun Run
 
-- Sample Results (captured by the Organiser who owns the related event)
INSERT INTO dbo.Result (enrolmentId, finishTime, position, capturedByOrganiserId) VALUES
(1, '04:15:32', 152, 1),  -- Sphiwe's Full Marathon result, captured by Onthatile
(3, '01:58:10', 47,  1);  -- Reitumetse's Half Marathon result, captured by Onthatile

SELECT * FROM Result