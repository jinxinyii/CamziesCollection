namespace CapstoneCC.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public int Discount { get; set; }
        public decimal Price { get; set; }
        public decimal RetailPrice { get; set; }
        public int RetailQuantity { get; set; }
        public string Description { get; set; }
        public string Badge { get; set; }
        public int Stock { get; set; }
        public string Barcode { get; set; }
    }
}
