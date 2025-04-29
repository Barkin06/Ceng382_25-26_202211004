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
        public string? Description { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
