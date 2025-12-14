# 🔒 Inventory Security Update

## Changes Applied
1.  **Stock In Button Removed**:
    *   Manual stock entry is now **disabled** for all users.
    *   **New Workflow**: Users must create a **Purchase Order** and mark it as "Received" to add stock. This ensures proper tracking.
2.  **Stock Out Restricted**:
    *   The "Stock Out" button (for damages/expiry) is now **Hidden** for normal Staff.
    *   It is **Visible only** to users with the **Admin** or **SuperAdmin** role.

## 🛠️ Verification
1.  **Staff View**: Log in as Staff. Go to Inventory. You should see **NO** Stock In/Out buttons.
2.  **Admin View**: Log in as Admin. Go to Inventory. You should see the **Stock Out** button (Orange).
