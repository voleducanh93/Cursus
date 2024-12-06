using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class WalletHistoryDTO
    {
        public int WalletId { get; set; }
        public double AmountChanged { get; set; }
        public double NewBalance { get; set; }
        public DateTime DateLogged { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
