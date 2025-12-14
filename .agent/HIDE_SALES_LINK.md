# ✅ Sales & Billing Link Hidden for Admins

## 🎯 Task
Hide the "Sales & Billing" navigation link for users with **Admin** or **SuperAdmin** roles.

## 🔧 Changes
I modified `DashboardLayout.razor` to wrap the Sales & Billing `NavLink` with a conditional check:

```razor
@if (userRole != "Admin" && userRole != "SuperAdmin")
{
    // Sales & Billing Link
}
```

## 🔍 Result
*   **Role: Staff / Pharmacist** → Can see and access Sales & Billing.
*   **Role: Admin / SuperAdmin** → Cannot see the link in the sidebar.

This ensures that administrative roles are not cluttered with point-of-sale operations that they do not perform.
