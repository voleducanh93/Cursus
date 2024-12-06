    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Cursus.Data.Entities
    {
        public class Category
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public DateTime DateCreated { get; set; }

            public bool Status { get; set; } = true;

            public bool IsParent { get; set; } = false;

            public string? ParentCategory {get; set;} = string.Empty;     
        
            public ICollection<Course> Courses { get; set; } = new List<Course>();
        }
    }
