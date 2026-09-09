namespace naif_katalog.Models
{
    public sealed class ConfirmOrderRequest
    {
        public string? CompanyName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<ConfirmOrderItem> Items { get; set; } = new();
    }

    public sealed class ConfirmOrderItem
    {
        public string? Code { get; set; }
        public string? Category { get; set; }
        public string? Price { get; set; }
        public string? Ayar { get; set; }
        public string? Renk { get; set; }
        public string? Gram { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }
        public List<ConfirmOrderStone> Stones { get; set; } = new();
    }

    public sealed class ConfirmOrderStone
    {
        public string? Type { get; set; }
        public string? Clarity { get; set; }
        public string? Color { get; set; }
        public string? Quantity { get; set; }
        public string? TotalCarat { get; set; }
    }
}
