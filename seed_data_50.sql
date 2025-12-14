
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1. Seed Suppliers (15 extra suppliers to mix with existing)
DECLARE @i INT = 1;
WHILE @i <= 15
BEGIN
    INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, City, Country, IsDeleted)
    VALUES (
        'Supplier ' + SUBSTRING(CAST(NEWID() AS VARCHAR(36)), 1, 8), 
        'Contact ' + CAST(@i AS VARCHAR), 
        'sup'+CAST(@i AS VARCHAR)+'@pharma.com', 
        '555-000'+CAST(@i AS VARCHAR), 
        '123 St', 'Metropolis', 'USA', 
        0
    );
    SET @i = @i + 1;
END

-- 2. Seed Medicines (50 items)
SET @i = 1;
WHILE @i <= 50
BEGIN
    DECLARE @SupName VARCHAR(100);
    SELECT TOP 1 @SupName = Name FROM Suppliers WHERE IsDeleted = 0 ORDER BY NEWID();
    
    DECLARE @Cat VARCHAR(50);
    SET @Cat = CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Antibiotics'
        WHEN 1 THEN 'Painkillers'
        WHEN 2 THEN 'Vitamins'
        WHEN 3 THEN 'Cardiac'
        ELSE 'Diabetes'
    END;

    INSERT INTO Medicines (Name, GenericName, Category, Quantity, MinQuantity, Price, Manufacturer, Batch, ExpiryDate, StorageLocation, IsDeleted)
    VALUES (
        'Med ' + SUBSTRING(CAST(NEWID() AS VARCHAR(50)), 1, 6), 
        'Gen ' + SUBSTRING(CAST(NEWID() AS VARCHAR(50)), 1, 4), 
        @Cat,
        (ABS(CHECKSUM(NEWID())) % 400) + 20, -- Qty 20-420
        20, 
        (ABS(CHECKSUM(NEWID())) % 90) + 5, -- Price 5-95
        @SupName,
        'B-' + CAST(@i AS VARCHAR),
        DATEADD(day, (ABS(CHECKSUM(NEWID())) % 700), GETDATE()), 
        'Shelf ' + CAST((ABS(CHECKSUM(NEWID())) % 10) AS VARCHAR),
        0
    );
    SET @i = @i + 1;
END

-- 3. Seed Sales (50 Transactions)
SET @i = 1;
WHILE @i <= 50
BEGIN
    DECLARE @SaleDate DATETIME = DATEADD(day, -(ABS(CHECKSUM(NEWID())) % 90), GETDATE()); -- Last 90 days
    DECLARE @Total DECIMAL(18,2) = 0;
    
    INSERT INTO Sales (SaleDate, PaymentMethod, SubTotal, DiscountAmount, TotalAmount, AmountPaid, IsDeleted)
    VALUES (@SaleDate, 'Cash', 0, 0, 0, 0, 0);
    
    DECLARE @NewSaleId INT = SCOPE_IDENTITY();
    
    -- Add 1-5 Items per Sale
    DECLARE @j INT = 1;
    DECLARE @NumItems INT = (ABS(CHECKSUM(NEWID())) % 5) + 1;
    
    WHILE @j <= @NumItems
    BEGIN
        DECLARE @MedId INT;
        DECLARE @Price DECIMAL(18,2);
        
        SELECT TOP 1 @MedId = MedicineId, @Price = Price FROM Medicines WHERE IsDeleted = 0 ORDER BY NEWID();
        
        DECLARE @Qty INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
        DECLARE @LineTotal DECIMAL(18,2) = @Qty * @Price;
        
        INSERT INTO SaleItems (SaleId, MedicineId, Quantity, UnitPrice, DiscountPct)
        VALUES (@NewSaleId, @MedId, @Qty, @Price, 0);
        
        SET @Total = @Total + @LineTotal;
        SET @j = @j + 1;
    END
    
    UPDATE Sales SET SubTotal = @Total, TotalAmount = @Total, AmountPaid = @Total WHERE SaleId = @NewSaleId;
    
    SET @i = @i + 1;
END

-- 4. Seed Purchase Orders (50 Orders)
SET @i = 1;
WHILE @i <= 50
BEGIN
    DECLARE @SupId INT;
    SELECT TOP 1 @SupId = SupplierId FROM Suppliers WHERE IsDeleted = 0 ORDER BY NEWID();
    
    DECLARE @OrderDate DATETIME = DATEADD(day, -(ABS(CHECKSUM(NEWID())) % 120), GETDATE()); -- Last 120 days
    
    INSERT INTO PurchaseOrders (SupplierId, OrderDate, ExpectedDate, Status, TotalAmount)
    VALUES (@SupId, @OrderDate, DATEADD(day, 7, @OrderDate), 'Received', 0);
    
    DECLARE @NewPOId INT = SCOPE_IDENTITY();
    DECLARE @POTotal DECIMAL(18,2) = 0;
    
    -- Add 1-5 Items per PO
    SET @j = 1;
    SET @NumItems = (ABS(CHECKSUM(NEWID())) % 5) + 1;
    
    WHILE @j <= @NumItems
    BEGIN
        DECLARE @MedIdPo INT;
        DECLARE @Cost DECIMAL(18,2); -- Assuming cost is somewhat related to Price (e.g. 70%)
        
        SELECT TOP 1 @MedIdPo = MedicineId, @Cost = Price * 0.7 FROM Medicines WHERE IsDeleted = 0 ORDER BY NEWID();
        
        DECLARE @QtyPo INT = (ABS(CHECKSUM(NEWID())) % 50) + 10;
        DECLARE @LineTotalPo DECIMAL(18,2) = @QtyPo * @Cost;
        
        INSERT INTO PurchaseOrderItems (PurchaseOrderId, MedicineId, QuantityOrdered, UnitPrice)
        VALUES (@NewPOId, @MedIdPo, @QtyPo, @Cost);
        
        SET @POTotal = @POTotal + @LineTotalPo;
        SET @j = @j + 1;
    END
    
    UPDATE PurchaseOrders SET TotalAmount = @POTotal WHERE PurchaseOrderId = @NewPOId;

    SET @i = @i + 1;
END
