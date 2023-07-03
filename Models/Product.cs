namespace elasticnest.Models
{
    public class Product
    {
        public required int id { get; set; }
        public required string Title { get; set; }
        public required String Description { get; set; }
        public int Price { get; set; } = 69;
        public int Quantity { get; set; } = 69;
    }
}