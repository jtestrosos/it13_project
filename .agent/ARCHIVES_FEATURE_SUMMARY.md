# 🗄️ Archives Feature Implemented

## 1. New Page: Archives
*   **URL**: `/archives`
*   **Access**: Admin & SuperAdmin
*   **Location**: Sidebar Menu (bottom)

## 2. Functionality
The page has 3 tabs:
1.  **Medicines**:
    *   Lists medicines where `IsDeleted = 1`.
    *   **Restore**: Sets `IsDeleted = 0`. Item reappears in Inventory.
2.  **Suppliers**:
    *   Lists suppliers where `IsDeleted = 1`.
    *   **Restore**: Sets `IsDeleted = 0`. Item reappears in Suppliers list.
3.  **Sales**:
    *   Lists sales where `IsDeleted = 1`.
    *   **Restore**:
        *   Sets `IsDeleted = 0`.
        *   **⚠️ IMPORTANT**: Deducts the items from Inventory stock again (re-applying the sale).

## 3. Technical Changes
*   Updated `IMedicineService`, `ISupplierService`, `ISaleService` with `GetArchived...` and `Restore...` methods.
*   Implemented logic in services.
*   Updated `DashboardLayout` sidebar.
