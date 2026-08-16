CREATE OR ALTER PROCEDURE Orders.GetAllOrders
AS
BEGIN
    SET NOCOUNT ON;

Select
    o.OrderId,
    o.OrderExternalID as [OrderExternalId],
        o.OrderDate,
        o.CreatedDate as [OrderCreatedDate],
        o.ModifiedDate as [OrderModifiedDate],
        ol.OrderID as [OrderLineId],
        ol.CustomerCode,
        ol.[LineNo],
        ol.Sku,
        ol.Quantity,
        ol.UnitPriceCents,
        ol.Currency,
        ol.CreateDate as [OrderLineCreateDate],
        ol.UpdateDate as [OrderLineUpdateDate],
        OV.OrderVersionId,
        OV.VersionNumber,
        OV.VersionDate
from Orders.Orders o
    inner join Orders.OrderLines OL on o.OrderId = OL.OrderID
    inner join Orders.OrderVersion OV on o.OrderId = OV.OrderId
END
GO