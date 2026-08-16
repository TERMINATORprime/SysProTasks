CREATE TYPE Orders.OrderTvp AS TABLE
    (
    OrderExternalID nvarchar(250) NOT NULL PRIMARY KEY,
    OrderDate       date          NOT NULL
    );
GO

CREATE TYPE Orders.OrderLineTvp AS TABLE
    (
    OrderLineID     uniqueidentifier NOT NULL,
    OrderExternalID nvarchar(250)    NOT NULL,
    [LineNo]        int              NOT NULL,
    CustomerCode    nvarchar(max)    NULL,
    Sku             nvarchar(64)     NOT NULL,
    Quantity        int              NOT NULL,
    UnitPriceCents  decimal(18,6)    NOT NULL,
    Currency        nchar(3)         NULL,
    PRIMARY KEY (OrderExternalID, [LineNo])
    );
GO

CREATE TYPE Orders.OrderVersionTvp AS TABLE
    (
    OrderExternalID nvarchar(250) NOT NULL PRIMARY KEY,
    VersionNumber   int           NOT NULL,
    VersionDate     datetime2     NOT NULL
    );
GO