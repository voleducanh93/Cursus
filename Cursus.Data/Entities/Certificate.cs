using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Data.Entities
{
    public class Certificate
    {
        [Key]
        public int Id { get; set; }
        public byte[] PdfData { get; set; }
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
