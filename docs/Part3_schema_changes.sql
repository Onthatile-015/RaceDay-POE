ALTER TABLE dbo.Event
ADD eventType VARCHAR(20) NOT NULL
    CONSTRAINT DF_Event_EventType DEFAULT 'Run'
    CONSTRAINT CK_Event_EventType CHECK (eventType IN ('Run','Walk','Cycle'));

ALTER TABLE dbo.Event
ADD bannerImageUrl NVARCHAR(500) NULL;

ALTER TABLE dbo.[User]
ADD profilePictureUrl NVARCHAR(500) NULL;