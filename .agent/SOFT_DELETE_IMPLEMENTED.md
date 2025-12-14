# 🗑️ Soft Delete (Archive Only) Implemented

## 🛡️ Database Changes
*   Added `IsDeleted` column (Bit/Boolean) to `Medicines`, `Suppliers`, and `Sales` tables.
*   Default value is `0` (False).

## 📁 Service Updates
The following services were updated to support "Archiving" instead of "Hard Delete":
1.  **MedicineService / InventoryService**:
    *   `Delete` -> Updates `IsDeleted = 1`.
    *   `GetAll` / `Validation` -> Filters `WHERE IsDeleted = 0`.
    *   Inventory Stats now exclude deleted items.
2.  **SupplierService**:
    *   `Delete` -> Updates `IsDeleted = 1`.
    *   `GetAll` -> Filters `WHERE IsDeleted = 0`.
3.  **SaleService**:
    *   `DeleteSale` -> Updates `IsDeleted = 1` (Archived).
    *   **Stock Restoration**: When you delete/archive a sale, the stock IS still restored to inventory (treating it as a voided sale).
    *   `GetRecentSales` -> Filters out archived sales.

## 📝 Usage
*   When you click "Delete" in the UI, the item will disappear from the list but remain in the database for historical/audit purposes.
*   To "Restore" an item, you would need to manually update the database (`UPDATE ... SET IsDeleted = 0`).
