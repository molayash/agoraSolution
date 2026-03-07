namespace CRM.Application.DTOs.Product
{
    public class ProductReviewDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public decimal? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
}
