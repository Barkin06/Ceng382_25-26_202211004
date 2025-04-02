using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week2.Models;
using System.Collections.Generic;
using System.Linq;

//I asked gpt: add filtering and pagination features my last time code that I shared with you last week.
//also filtering will be done on the data list in the backend.
//The filtering logic must be written inside the OnGet methods.
// I will create a new model class called ClassInformationTable
// This model will store the filtered version of the main model.
//In this model, the ID should not be shown in the table,
// but the ID will still be used in the background for actions like edit, delete, or details. 
//In addition to filtering, you are required to implement pagination.
//create a synthetic data for 100 samples


namespace Week2.Pages
{
    public class IndexModel : PageModel
    {
        private static List<ClassInformationModel> _classList;

        public static List<ClassInformationModel> ClassList
        {
            get
            {
                if (_classList == null)
                    _classList = GenerateSampleData();
                return _classList;
            }
        }

        [BindProperty]
        public ClassInformationModel NewClass { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public ClassInformationModel EditingClass { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterClassName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public List<ClassInformationTable> FilteredList { get; set; } = new();

        public void OnGet()
        {
            var query = ClassList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterClassName))
            {
                query = query.Where(c => c.ClassName.ToLower().Contains(FilterClassName.ToLower()));
            }

            TotalPages = (int)System.Math.Ceiling(query.Count() / (double)PageSize);
            var paged = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            FilteredList = paged.Select(c => new ClassInformationTable
            {
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description,
                HiddenId = c.Id
            }).ToList();

            if (EditId.HasValue)
            {
                EditingClass = ClassList.FirstOrDefault(c => c.Id == EditId.Value);
                if (EditingClass != null)
                {
                    NewClass = new ClassInformationModel(EditingClass.Id)
                    {
                        ClassName = EditingClass.ClassName,
                        StudentCount = EditingClass.StudentCount,
                        Description = EditingClass.Description
                    };
                }
            }
        }

        public IActionResult OnPostAdd() // It has some bugs inside, for me to understand in better way, 
                                        // help me put some print states gpt
        {
            ModelState.Remove(nameof(FilterClassName));
            Console.WriteLine("➡️ OnPostAdd tetiklendi");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"NewClass = {(NewClass == null ? "null" : NewClass.ClassName)}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState geçersiz.");
                foreach (var kv in ModelState)
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        Console.WriteLine($"🚨 Hata -> {kv.Key}: {err.ErrorMessage}");
                    }
                }
                return Page();
            }

            NewClass.Id = ClassList.Any() ? ClassList.Max(c => c.Id) + 1 : 1;
            Console.WriteLine($"✅ Class eklendi: {NewClass.ClassName}, ID: {NewClass.Id}");

            ClassList.Add(NewClass);
            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(c => c.Id == id);
            if (item != null)
                ClassList.Remove(item);

            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public IActionResult OnPostEdit()
        {
            if (!EditId.HasValue)
                return RedirectToPage(new { FilterClassName, PageNumber });

            var item = ClassList.FirstOrDefault(c => c.Id == EditId.Value);
            if (item != null && ModelState.IsValid)
            {
                item.ClassName = NewClass.ClassName;
                item.StudentCount = NewClass.StudentCount;
                item.Description = NewClass.Description;

                Console.WriteLine($"[EDIT] Updated class: ID={item.Id}, Name={item.ClassName}");
            }

            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        private static List<ClassInformationModel> GenerateSampleData()
        {
            var list = new List<ClassInformationModel>();
            for (int i = 1; i <= 100; i++)
            {
                list.Add(new ClassInformationModel
                {
                    ClassName = $"Class {i}",
                    StudentCount = 20 + (i % 30),
                    Description = $"Description for Class {i}"
                });
            }
            return list;
        }
    }
}
