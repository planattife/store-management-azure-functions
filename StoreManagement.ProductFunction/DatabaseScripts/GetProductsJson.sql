CREATE FUNCTION fn_GetProductsJson(@StoreId INT)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @json NVARCHAR(MAX);

    SELECT @json = (
        SELECT ProductId, Name, Description, Price, CreatedAt, UpdatedAt
        FROM Product
        WHERE StoreId = @StoreId
        FOR JSON AUTO
    );

    RETURN @json;
END;