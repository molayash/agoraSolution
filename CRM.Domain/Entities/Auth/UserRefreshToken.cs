using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Auth
{
    [Table("UserRefreshToken")]
    public class UserRefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserRefreshTokenId { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")] public virtual ApplicationUser User { get; set; }

        public string RefreshToken { get; set; }

        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        public DateTime ExpiryDate { get; set; }
        public string? OTP { get; set; }
    }
}
