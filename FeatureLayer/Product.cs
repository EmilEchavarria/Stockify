namespace FeatureLayer
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
        public byte[] Image { get; set; }

        // Constructor
        public Product(int productId, string productName, string description, decimal price, int stock, string status, byte[] image)
        {
            ProductID = productId;
            ProductName = productName;
            Description = description;
            Price = price;
            Stock = stock;
            Status = status;
            Image = null;
        }

        // Constructor sin ID (para el insert)
        public Product(string productName, string description, decimal price, int stock, string status, byte[] image)
        {
            ProductName = productName;
            Description = description;
            Price = price;
            Stock = stock;
            Status = status;
            Image = image;
        }
    }
}
