using System.ComponentModel.DataAnnotations;

namespace Week2.Models
{
    public class ClassInformationModel
    {
        private static int _autoId = 1;
        public ClassInformationModel() => Id = _autoId++;

        public ClassInformationModel(int id) => Id = id;

        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Class Name is required.")]
        public string ClassName { get; set; }

        [Range(1, 100000000000, ErrorMessage = "Student Count must be positive.")]
        public int StudentCount { get; set; }

        public string Description { get; set; }
    }
}