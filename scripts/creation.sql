-- 1. Create database (optional if you already created it)
IF DB_ID('EventSpark') IS NULL
BEGIN
    CREATE DATABASE EventSparkDb;
END
GO

USE EventSpark;
GO

/* 
===========================================================
  TABLE: EventCategories
===========================================================
*/
IF OBJECT_ID('dbo.EventCategories', 'U') IS NOT NULL
    DROP TABLE dbo.EventCategories;
GO

CREATE TABLE dbo.EventCategories
(
    EventCategoryId INT IDENTITY(1,1) CONSTRAINT PK_EventCategories PRIMARY KEY,
    Name            NVARCHAR(100) NOT NULL,
    Slug            NVARCHAR(100) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_EventCategories_CreatedAt DEFAULT SYSUTCDATETIME()
);

--unique index on category name / slug
CREATE UNIQUE INDEX UX_EventCategories_Name ON dbo.EventCategories(Name);
GO


/* 
===========================================================
  TABLE: Events
===========================================================
*/
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;
GO

CREATE TABLE dbo.Events
(
    EventId         INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    OrganizerId     NVARCHAR(450) NOT NULL,  
    CategoryId      INT NULL,

    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NOT NULL,

    VenueName       NVARCHAR(200) NOT NULL,
    VenueAddress    NVARCHAR(400) NULL,
    City            NVARCHAR(100) NOT NULL,

    StartDateTime   DATETIME2(0) NOT NULL,
    EndDateTime     DATETIME2(0) NOT NULL,

    -- Status: 0 = Draft, 1 = Published, 2 = Cancelled
    Status          TINYINT NOT NULL CONSTRAINT DF_Events_Status DEFAULT(0),

    ImagePath       NVARCHAR(400) NULL,
    MaxCapacity     INT NULL,

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Events_UpdatedAt DEFAULT SYSUTCDATETIME(),

    RowVersion      ROWVERSION NOT NULL
);

-- FK to categories
ALTER TABLE dbo.Events
ADD CONSTRAINT FK_Events_EventCategories
FOREIGN KEY (CategoryId) REFERENCES dbo.EventCategories(EventCategoryId);

-- Check: End must be after Start
ALTER TABLE dbo.Events
ADD CONSTRAINT CHK_Events_StartEnd
CHECK (EndDateTime > StartDateTime);

-- Check: Status in allowed range
ALTER TABLE dbo.Events
ADD CONSTRAINT CHK_Events_Status
CHECK (Status IN (0,1,2));

-- Useful indexes
CREATE INDEX IX_Events_OrganizerId ON dbo.Events(OrganizerId);
CREATE INDEX IX_Events_CategoryId ON dbo.Events(CategoryId);
CREATE INDEX IX_Events_StartDateTime ON dbo.Events(StartDateTime);
GO


/* 
===========================================================
  TABLE: TicketTypes
===========================================================
*/
IF OBJECT_ID('dbo.TicketTypes', 'U') IS NOT NULL
    DROP TABLE dbo.TicketTypes;
GO

CREATE TABLE dbo.TicketTypes
(
    TicketTypeId    INT IDENTITY(1,1) CONSTRAINT PK_TicketTypes PRIMARY KEY,
    EventId         INT NOT NULL,

    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(400) NULL,

    Price           DECIMAL(10,2) NOT NULL,
    TotalQuantity   INT NOT NULL,

    SaleStartUtc    DATETIME2(0) NULL,
    SaleEndUtc      DATETIME2(0) NULL,

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_TicketTypes_CreatedAt DEFAULT SYSUTCDATETIME(),
    RowVersion      ROWVERSION NOT NULL
);

ALTER TABLE dbo.TicketTypes
ADD CONSTRAINT FK_TicketTypes_Events
FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);

-- Checks
ALTER TABLE dbo.TicketTypes
ADD CONSTRAINT CHK_TicketTypes_Price
CHECK (Price >= 0);

ALTER TABLE dbo.TicketTypes
ADD CONSTRAINT CHK_TicketTypes_TotalQuantity
CHECK (TotalQuantity >= 0);

ALTER TABLE dbo.TicketTypes
ADD CONSTRAINT CHK_TicketTypes_SaleDates
CHECK (SaleEndUtc IS NULL OR SaleStartUtc IS NULL OR SaleEndUtc > SaleStartUtc);

CREATE INDEX IX_TicketTypes_EventId ON dbo.TicketTypes(EventId);
GO


/* 
===========================================================
  TABLE: Orders
===========================================================
*/
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
    DROP TABLE dbo.Orders;
GO

CREATE TABLE dbo.Orders
(
    OrderId         INT IDENTITY(1,1) CONSTRAINT PK_Orders PRIMARY KEY,

    BuyerId         NVARCHAR(450) NOT NULL,  -- later map to AspNetUsers.Id

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),

    -- Status: 0 = Pending, 1 = Paid, 2 = Cancelled, 3 = Refunded
    Status          TINYINT NOT NULL CONSTRAINT DF_Orders_Status DEFAULT(1),

    TotalAmount     DECIMAL(10,2) NOT NULL,

    PaymentReference NVARCHAR(100) NULL,

    -- Snapshots of buyer data at purchase time
    EmailSnapshot   NVARCHAR(256) NULL,
    FullNameSnapshot NVARCHAR(200) NULL
);

ALTER TABLE dbo.Orders
ADD CONSTRAINT CHK_Orders_Status
CHECK (Status IN (0,1,2,3));

ALTER TABLE dbo.Orders
ADD CONSTRAINT CHK_Orders_TotalAmount
CHECK (TotalAmount >= 0);

CREATE INDEX IX_Orders_BuyerId ON dbo.Orders(BuyerId);
CREATE INDEX IX_Orders_CreatedAt ON dbo.Orders(CreatedAt);
GO


/* 
===========================================================
  TABLE: OrderItems
===========================================================
*/
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL
    DROP TABLE dbo.OrderItems;
GO

CREATE TABLE dbo.OrderItems
(
    OrderItemId             INT IDENTITY(1,1) CONSTRAINT PK_OrderItems PRIMARY KEY,
    OrderId                 INT NOT NULL,
    TicketTypeId            INT NOT NULL,

    Quantity                INT NOT NULL,
    UnitPrice               DECIMAL(10,2) NOT NULL,

    TicketTypeNameSnapshot  NVARCHAR(100) NOT NULL,

    -- Calculated column
    LineTotal AS (Quantity * UnitPrice) PERSISTED
);

ALTER TABLE dbo.OrderItems
ADD CONSTRAINT FK_OrderItems_Orders
FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId);

ALTER TABLE dbo.OrderItems
ADD CONSTRAINT FK_OrderItems_TicketTypes
FOREIGN KEY (TicketTypeId) REFERENCES dbo.TicketTypes(TicketTypeId);

ALTER TABLE dbo.OrderItems
ADD CONSTRAINT CHK_OrderItems_Quantity
CHECK (Quantity > 0);

ALTER TABLE dbo.OrderItems
ADD CONSTRAINT CHK_OrderItems_UnitPrice
CHECK (UnitPrice >= 0);

CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
CREATE INDEX IX_OrderItems_TicketTypeId ON dbo.OrderItems(TicketTypeId);
GO


/* 
===========================================================
  TABLE: Tickets
===========================================================
*/
IF OBJECT_ID('dbo.Tickets', 'U') IS NOT NULL
    DROP TABLE dbo.Tickets;
GO

CREATE TABLE dbo.Tickets
(
    TicketId            INT IDENTITY(1,1) CONSTRAINT PK_Tickets PRIMARY KEY,
    OrderItemId         INT NOT NULL,
    EventId             INT NOT NULL,

    TicketNumber        NVARCHAR(50) NOT NULL,
    QrCodeValue         NVARCHAR(200) NOT NULL,

    -- Ticket Status: 0 = Active, 1 = Cancelled, 2 = Refunded
    Status              TINYINT NOT NULL CONSTRAINT DF_Tickets_Status DEFAULT(0),

    CreatedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT SYSUTCDATETIME(),
    CheckedInAt         DATETIME2(0) NULL,

    CheckedInByUserId   NVARCHAR(450) NULL,  -- door staff/organizer user id

    RowVersion          ROWVERSION NOT NULL
);

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_OrderItems
FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(OrderItemId);

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Events
FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);

ALTER TABLE dbo.Tickets
ADD CONSTRAINT CHK_Tickets_Status
CHECK (Status IN (0,1,2));

-- One ticket number / QR must be unique system-wide
CREATE UNIQUE INDEX UX_Tickets_TicketNumber ON dbo.Tickets(TicketNumber);
CREATE UNIQUE INDEX UX_Tickets_QrCodeValue ON dbo.Tickets(QrCodeValue);

CREATE INDEX IX_Tickets_EventId ON dbo.Tickets(EventId);
CREATE INDEX IX_Tickets_OrderItemId ON dbo.Tickets(OrderItemId);
GO


/* 
===========================================================
  TABLE: CheckInLogs
===========================================================
*/
IF OBJECT_ID('dbo.CheckInLogs', 'U') IS NOT NULL
    DROP TABLE dbo.CheckInLogs;
GO

CREATE TABLE dbo.CheckInLogs
(
    CheckInLogId        INT IDENTITY(1,1) CONSTRAINT PK_CheckInLogs PRIMARY KEY,
    TicketId            INT NULL,      -- null if QR code invalid / not found
    EventId             INT NOT NULL,

    ScannedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_CheckInLogs_ScannedAt DEFAULT SYSUTCDATETIME(),
    ScannedByUserId     NVARCHAR(450) NULL,  -- staff/organizer who scanned

    -- Result: 0 = Success, 1 = AlreadyCheckedIn, 2 = InvalidCode, 3 = CancelledTicket, 4 = Other
    Result              TINYINT NOT NULL,

    RawCode             NVARCHAR(200) NULL,
    Message             NVARCHAR(400) NULL
);

ALTER TABLE dbo.CheckInLogs
ADD CONSTRAINT FK_CheckInLogs_Tickets
FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId);

ALTER TABLE dbo.CheckInLogs
ADD CONSTRAINT FK_CheckInLogs_Events
FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);

ALTER TABLE dbo.CheckInLogs
ADD CONSTRAINT CHK_CheckInLogs_Result
CHECK (Result IN (0,1,2,3,4));

CREATE INDEX IX_CheckInLogs_EventId ON dbo.CheckInLogs(EventId);
CREATE INDEX IX_CheckInLogs_TicketId ON dbo.CheckInLogs(TicketId);
CREATE INDEX IX_CheckInLogs_ScannedAt ON dbo.CheckInLogs(ScannedAt);
GO
