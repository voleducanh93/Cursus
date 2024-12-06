using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
        [ForeignKey("ApplicationUser")]
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
        public string GroupName { get; set; }
    }
}
