CREATE PROCEDURE sp_InsertProduct
    @StoreId INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(255),
    @Price DECIMAL(18,2)
AS
BEGIN
    INSERT INTO Product (StoreId, Name, Description, Price)
    VALUES (@StoreId, @Name, @Description, @Price);
END;