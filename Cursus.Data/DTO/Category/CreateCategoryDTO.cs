using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO.Category
{
    public class CreateCategoryDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool Status { get; set; } = true;

        public bool IsParent { get; set; } = false;

        public string? ParentCategory { get; set; }

    }
}
