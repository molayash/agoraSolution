namespace CRM.Application.DTOs.Product
{
    public class ProductColorDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string? Color { get; set; }
        public string? Name { get; set; }
        public string? ColorCode { get; set; }
    }
}
