# 🛑 Safety Check: Removing Stock Manual Adjustments

## 1. Stock Out (Manual)
**⚠️ NOT SAFE TO REMOVE**
*   **Why?**: You need a way to remove stock that wasn't "sold" (e.g., **Damaged**, **Expired**, **Lost/Stolen**, or **Correction**).
*   **Sales vs. Stock Out**: The "Sales" page handles customer purchases. The "Stock Out" button handles *everything else*.
*   **Recommendation**: Keep this button.

## 2. Stock In (Manual)
**✅ SAFE TO REMOVE (Conditional)**
*   **Why?**: You now have a **Purchase Order (PO)** system. When you "Receive" a PO, it automatically adds stock. Using POs is better for auditing and tracking costs.
*   **Caveat**: You might still want a manual "Stock In" for **Corrections** (e.g., "We counted 10 items but system says 5").
*   **Recommendation**: You can remove it to force staff to use POs, preventing un-tracked stock entry. Or, rename it to "Correction (+)".

## 💡 Proposed Action
If you want to clean up the UI:
1.  **Remove "Stock In"**: Rely 100% on Purchase Orders.
2.  **Keep "Stock Out"**: Rename it to "Log Damage/Loss".
