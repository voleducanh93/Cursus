using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool Status { get; set; } = true;

        public string? Address { get; set; }

        public ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
    }
}
