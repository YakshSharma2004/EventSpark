USE EventSpark;
GO

-- AspNetUsers
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers (
        Id                   NVARCHAR(450) NOT NULL PRIMARY KEY,
        UserName             NVARCHAR(256) NULL,
        NormalizedUserName   NVARCHAR(256) NULL,
        Email                NVARCHAR(256) NULL,
        NormalizedEmail      NVARCHAR(256) NULL,
        EmailConfirmed       BIT NOT NULL DEFAULT(0),
        PasswordHash         NVARCHAR(MAX) NULL,
        SecurityStamp        NVARCHAR(MAX) NULL,
        ConcurrencyStamp     NVARCHAR(MAX) NULL,
        PhoneNumber          NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL DEFAULT(0),
        TwoFactorEnabled     BIT NOT NULL DEFAULT(0),
        LockoutEnd           DATETIMEOFFSET(7) NULL,
        LockoutEnabled       BIT NOT NULL DEFAULT(0),
        AccessFailedCount    INT NOT NULL DEFAULT(0)
    );

    CREATE UNIQUE INDEX UX_AspNetUsers_NormalizedUserName
        ON dbo.AspNetUsers (NormalizedUserName)
        WHERE NormalizedUserName IS NOT NULL;

    CREATE INDEX IX_AspNetUsers_NormalizedEmail
        ON dbo.AspNetUsers (NormalizedEmail);
END
GO

-- AspNetRoles
IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles (
        Id               NVARCHAR(450) NOT NULL PRIMARY KEY,
        Name             NVARCHAR(256) NULL,
        NormalizedName   NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL
    );

    CREATE UNIQUE INDEX UX_AspNetRoles_NormalizedName
        ON dbo.AspNetRoles (NormalizedName)
        WHERE NormalizedName IS NOT NULL;
END
GO

-- AspNetUserRoles
IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_Users
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_Roles
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserRoles_RoleId
        ON dbo.AspNetUserRoles(RoleId);
END
GO

-- AspNetUserLogins
IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserLogins (
        LoginProvider       NVARCHAR(450) NOT NULL,
        ProviderKey         NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId              NVARCHAR(450) NOT NULL,

        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_Users
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserLogins_UserId
        ON dbo.AspNetUserLogins(UserId);
END
GO

-- AspNetUserClaims
IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserClaims (
        Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId     NVARCHAR(450) NOT NULL,
        ClaimType  NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetUserClaims_Users
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserClaims_UserId
        ON dbo.AspNetUserClaims(UserId);
END
GO

-- AspNetRoleClaims
IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims (
        Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId     NVARCHAR(450) NOT NULL,
        ClaimType  NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetRoleClaims_Roles
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetRoleClaims_RoleId
        ON dbo.AspNetRoleClaims(RoleId);
END
GO

-- AspNetUserTokens
IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserTokens (
        UserId        NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name          NVARCHAR(450) NOT NULL,
        Value         NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_Users
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO
