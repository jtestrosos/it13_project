// New class: Supplier.cs (or placed in the @code block)
public class Supplier
{
	// Primary Key (matches SQL column: SupplierId)
	public int SupplierId { get; set; }
	public string Name { get; set; } = "";
	// We only need Id and Name for the dropdown list
}