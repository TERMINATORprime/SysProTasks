CREATE OR ALTER PROCEDURE Orders.InsertOrUpdateOrders
    @Orders        Orders.OrderTvp        READONLY,
    @OrderLines    Orders.OrderLineTvp    READONLY,
    @OrderVersions Orders.OrderVersionTvp READONLY
    AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

BEGIN TRAN;

UPDATE o
SET o.OrderDate    = s.OrderDate,
    o.ModifiedDate = SYSUTCDATETIME()
    FROM Orders.Orders o
             JOIN @Orders s ON s.OrderExternalID = o.OrderExternalID;

INSERT INTO Orders.Orders (OrderExternalID, OrderDate)
SELECT s.OrderExternalID, s.OrderDate
FROM @Orders s
WHERE NOT EXISTS (SELECT 1 FROM Orders.Orders o WHERE o.OrderExternalID = s.OrderExternalID);

UPDATE ol
SET ol.CustomerCode   = l.CustomerCode,
    ol.Sku            = l.Sku,
    ol.Quantity       = l.Quantity,
    ol.UnitPriceCents = l.UnitPriceCents,
    ol.Currency       = l.Currency,
    ol.UpdateDate     = SYSUTCDATETIME()
    FROM Orders.OrderLines ol
        JOIN Orders.Orders o ON o.OrderId = ol.OrderID
    JOIN @OrderLines l   ON l.OrderExternalID = o.OrderExternalID
    AND ol.OrderLineId = l.OrderLineId
    AND l.[LineNo] = ol.[LineNo];

INSERT INTO Orders.OrderLines (CustomerCode, [LineNo], Sku, Quantity, UnitPriceCents, Currency, OrderID)
SELECT l.CustomerCode, l.[LineNo], l.Sku, l.Quantity, l.UnitPriceCents, l.Currency, o.OrderId
FROM @OrderLines l
         JOIN Orders.Orders o ON o.OrderExternalID = l.OrderExternalID
WHERE NOT EXISTS (
    SELECT 1 FROM Orders.OrderLines ol
    WHERE ol.OrderID = o.OrderId AND ol.[LineNo] = l.[LineNo]
);

INSERT INTO Orders.OrderVersion (OrderId, VersionNumber, VersionDate)
SELECT o.OrderId, v.VersionNumber, v.VersionDate
FROM @OrderVersions v
         JOIN Orders.Orders o ON o.OrderExternalID = v.OrderExternalID;

COMMIT TRAN;

SELECT o.OrderId, o.OrderExternalID, o.OrderDate, o.CreatedDate, o.ModifiedDate
FROM Orders.Orders o
         JOIN @Orders s ON s.OrderExternalID = o.OrderExternalID;
END
GO