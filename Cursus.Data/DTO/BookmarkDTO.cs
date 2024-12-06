using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class BookmarkDTO
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public double Price { get; set; }
        public double Rating { get; set; }
    }
}
