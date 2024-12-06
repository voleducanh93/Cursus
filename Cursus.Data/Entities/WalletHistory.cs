using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class WalletHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [ForeignKey("WalletId")]
        public Wallet Wallet { get; set; }

        [Required]
        public double AmountChanged { get; set; }

        [Required]
        public double NewBalance { get; set; }

        [Required]
        public DateTime DateLogged { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Description { get; set; }
    }
}
