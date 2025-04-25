namespace CapstoneCC.Models
{
    public class OrderModel
    {
        public class Order
        {
            public int OrderId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public DateTime OrderDate { get; set; }
            public string OrderStatus { get; set; } = "Pending";
            public List<OrderProduct> OrderProducts { get; set; }
            public List<OrderProduct> Products { get; set; } = new List<OrderProduct>();
        }

        public class OrderProduct   
        {
            public int OrderProductId { get; set; } // Primary Key
            public int OrderId { get; set; } // Foreign Key
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Total => Price * Quantity;
            public Order Order { get; set; } // Links to the parent order
            public Product Product { get; set; }
        }
    }
}