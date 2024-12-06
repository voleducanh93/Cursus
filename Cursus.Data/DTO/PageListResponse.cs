using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class PageListResponse<T>
    {
        
            public List<T> Items { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public bool HasNextPage { get; set; }
            public bool HasPreviousPage { get; set; }
        
    }
}
