CREATE OR ALTER PROCEDURE Orders.GetOrderSummary
    @StartDate datetime,
    @EndDate datetime = GETUTCDATE
    AS
BEGIN
    SET NOCOUNT ON;

Select
    ol.CustomerCode,
    SUM(ol.Quantity * ol.UnitPriceCents) as Total
from Orders.Orders o
         inner join Orders.OrderLines OL on o.OrderId = OL.OrderID
         inner join Orders.OrderVersion OV on o.OrderId = OV.OrderId
where o.OrderDate >= @StartDate and o.OrderDate <= @EndDate
GROUP BY ol.CustomerCode
ORDER BY CustomerCode
END
GO