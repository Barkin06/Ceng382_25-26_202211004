using System.ComponentModel.DataAnnotations;
using  Week2.Models;

namespace Week2.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int PersonCount { get; set; }
        public bool IsActive { get; set; }

        public int ClassId { get; set; }  // Primary key
        public string ClassName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
