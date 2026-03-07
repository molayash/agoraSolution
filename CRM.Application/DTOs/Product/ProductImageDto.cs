namespace CRM.Application.DTOs.Product
{
    public class ProductImageDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
