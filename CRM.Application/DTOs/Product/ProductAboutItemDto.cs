namespace CRM.Application.DTOs.Product
{
    public class ProductAboutItemDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string? AboutItem { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
