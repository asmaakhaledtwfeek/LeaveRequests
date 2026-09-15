-- ============================================================
-- Leave Requests DB setup
-- Run this script against an existing SQL Server instance.
-- Server: ASMAA_KHALED  |  Auth: Windows Authentication
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LeaveRequestsDb')
BEGIN
    CREATE DATABASE LeaveRequestsDb;
END
GO

USE LeaveRequestsDb;
GO

IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NOT NULL
    DROP TABLE dbo.LeaveRequests;
GO

CREATE TABLE dbo.LeaveRequests (
    Id           INT           IDENTITY(1,1)  PRIMARY KEY,
    EmployeeId   INT           NOT NULL,
    StartDate    DATE          NOT NULL,
    EndDate      DATE          NOT NULL,
    Type         NVARCHAR(20)  NOT NULL,
    Status       NVARCHAR(20)  NOT NULL  DEFAULT 'Pending',
    CreatedAt    DATETIME2     NOT NULL  DEFAULT GETUTCDATE(),
    ReviewerNote NVARCHAR(500) NULL,

    CONSTRAINT CK_LeaveRequests_EndDate
        CHECK (EndDate >= StartDate),

    CONSTRAINT CK_LeaveRequests_Status
        CHECK (Status IN ('Pending', 'Approved', 'Rejected')),

    CONSTRAINT CK_LeaveRequests_Type
        CHECK (Type IN ('Vacation', 'Sick', 'Unpaid'))
);
GO

INSERT INTO dbo.LeaveRequests (EmployeeId, StartDate, EndDate, Type, Status, CreatedAt, ReviewerNote)
VALUES
    (1,  '2025-01-06', '2025-01-10', 'Vacation', 'Approved', '2024-12-20 09:00:00', 'Approved – enjoy the break.'),
    (2,  '2025-01-13', '2025-01-14', 'Sick',     'Approved', '2025-01-13 08:15:00', NULL),
    (3,  '2025-01-20', '2025-01-24', 'Vacation', 'Rejected', '2025-01-10 11:30:00', 'Team headcount too low that week.'),
    (4,  '2025-02-03', '2025-02-07', 'Unpaid',   'Approved', '2025-01-25 14:00:00', NULL),
    (5,  '2025-02-10', '2025-02-11', 'Sick',     'Pending',  '2025-02-10 07:45:00', NULL),
    (6,  '2025-02-17', '2025-02-21', 'Vacation', 'Pending',  '2025-02-05 16:20:00', NULL),
    (7,  '2025-03-03', '2025-03-07', 'Vacation', 'Approved', '2025-02-20 10:10:00', NULL),
    (8,  '2025-03-10', '2025-03-10', 'Sick',     'Approved', '2025-03-10 08:00:00', NULL),
    (9,  '2025-03-17', '2025-03-21', 'Unpaid',   'Pending',  '2025-03-08 13:45:00', NULL),
    (10, '2025-04-07', '2025-04-11', 'Vacation', 'Pending',  '2025-03-28 09:30:00', NULL);
GO

SELECT * FROM dbo.LeaveRequests ORDER BY CreatedAt;
GO
