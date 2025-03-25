//sayfanın sol tarafında Class Name Student Count ve Description alanlarını alan bir form, 
// sağ tarafında ise gönderilen verileri gösteren bir tablo yer alacaktır. 
// Tablo, Id, Class Name, Student Count, Description ve Actions Edit ve Delete sütunlarından oluşacak.
//  Her form gönderildiğinde veriler doğrulanarak statik bir listeye eklenecek. 
// Bootstrap kullanılarak iki sütunlu bir yapı oluşturulacak, yalnızca Razor Pages ve C# metodları kullanılacak, 
// JavaScript kullanma. Form doğrulaması [Required], [Range] gibi C# attributeleriyle yapılacak,
//  düzenleme sırasında formu önceden doldur sonra güncelle formatına getir


//This tells Razor which C# method to run when clicking the button. For example, 
// asp-page- handler="Delete" triggers the OnPostDelete(int id) method in the PageModel.
//You need to bind the input and submit tags to C# IActionResults.
//Inside the cshtml.cs file, write these functions for the given example.
//public IActionResult OnPostAdd()
//{
//}
//public IActionResult OnPostDelete(int id)
//{
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week2.Models;
using System.Collections.Generic;
using System.Linq;

namespace Week2.Pages
{
    public class IndexModel : PageModel
    {
        // Statik liste (in-memory)
        public static List<ClassInformationModel> ClassList { get; set; } = new();

        // Formdan gelen veriler
        [BindProperty]
        public ClassInformationModel NewClass { get; set; }

        // Edit işleminde kullanılacak Id (Query string & Post'ta bağlanabilir)
        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        // Sadece sayfada göstermek için (Edit edilmekte olan nesne)
        public ClassInformationModel EditingClass { get; set; }

        public void OnGet()
        {
            // Eğer editId query string'de geldiyse
            if (EditId.HasValue)
            {
                EditingClass = ClassList.FirstOrDefault(c => c.Id == EditId.Value);
                if (EditingClass != null)
                {
                    // Formu doldurmak için mevcut verileri "NewClass" içine kopyalıyoruz
                    NewClass = new ClassInformationModel
                    {
                        ClassName = EditingClass.ClassName,
                        StudentCount = EditingClass.StudentCount,
                        Description = EditingClass.Description
                    };
                }
            }
        }

        public IActionResult OnPostAdd()
        {
            // Validasyon hatası varsa sayfa geri gelsin
            if (!ModelState.IsValid)
                return Page();

            ClassList.Add(NewClass);
            return RedirectToPage(); // Yeniden sayfayı yükle
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                ClassList.Remove(item);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            // Eğer EditId gelmediyse, doğrudan sayfaya dön
            if (!EditId.HasValue)
                return RedirectToPage();

            var item = ClassList.FirstOrDefault(c => c.Id == EditId.Value);
            if (item != null && ModelState.IsValid)
            {
                // Güncelleme
                item.ClassName = NewClass.ClassName;
                item.StudentCount = NewClass.StudentCount;
                item.Description = NewClass.Description;
            }
            return RedirectToPage();
        }
    }
}
