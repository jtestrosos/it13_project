-- Run this script in your SQL Server database to create the tracking tables

CREATE TABLE StockIn (
    StockInId INT PRIMARY KEY IDENTITY(1,1),
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
    DeliveryDate DATETIME NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId) ON DELETE CASCADE
);

CREATE TABLE StockOut (
    StockOutId INT PRIMARY KEY IDENTITY(1,1),
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
    Reason NVARCHAR(100) NOT NULL, -- 'Damaged', 'Expired', 'Lost', 'Correction'
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId) ON DELETE CASCADE
);
