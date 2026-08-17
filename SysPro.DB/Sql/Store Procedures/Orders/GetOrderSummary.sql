CREATE OR ALTER PROCEDURE Orders.GetOrderSummary
    @StartDate datetime2,
    @EndDate   datetime2 = NULL
    AS
BEGIN
    SET NOCOUNT ON;

    SET @EndDate = ISNULL(@EndDate, SYSUTCDATETIME());

Select
    ol.CustomerCode,
    SUM(CAST(ol.Quantity AS bigint) * ol.UnitPriceCents) as Total
from Orders.Orders o
         inner join Orders.OrderLines OL on o.OrderId = OL.OrderID
         inner join Orders.OrderVersion OV on o.OrderId = OV.OrderId
where o.OrderDate >= @StartDate and o.OrderDate <= @EndDate
GROUP BY ol.CustomerCode
ORDER BY CustomerCode
END
GO