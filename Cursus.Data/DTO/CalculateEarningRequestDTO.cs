using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class CalculateEarningRequestDTO
    {
        public int CourseId { get; set; }

        [DefaultValue(1)]
        public int Months { get; set; } = 1;
    }
}
