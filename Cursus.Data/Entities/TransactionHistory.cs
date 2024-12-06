using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class TransactionHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public ApplicationUser? User{ get; set; }

        public int? TrackedTransactionId { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

    }
}
