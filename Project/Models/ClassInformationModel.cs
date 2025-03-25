//GPT: Index sayfası üzerinde çalışılacak ve proje klasörü içine Models adında bir klasör oluşturulup, 
// içerisine Id, ClassName, StudentCount ve Description özelliklerine sahip ClassInformationModel.cs sınıfı yazılacaktır. 
// Id özelliği her eklemede otomatik artacak.



using System.ComponentModel.DataAnnotations;

namespace Week2.Models
{
    public class ClassInformationModel
    {
        public static int _nextId = 1;

        public ClassInformationModel()
        {
            Id = _nextId++;
        }

        public int Id { get; private set; }

        [Required(ErrorMessage = "Class Name is required")]
        public string ClassName { get; set; }

        [Range(1, 1000, ErrorMessage = "Student Count must be between 1 and 1000")]
        public int StudentCount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
    }
}
