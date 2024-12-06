using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Wallet
    {
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public double? Balance { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
