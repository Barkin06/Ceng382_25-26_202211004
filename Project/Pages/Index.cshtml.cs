using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week2.Models;
using Week2.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        public IActionResult OnPostAdd()
        {
            ModelState.Remove(nameof(FilterClassName));
            if (!ModelState.IsValid)
                return Page();

            NewClass.Id = ClassList.Any() ? ClassList.Max(c => c.Id) + 1 : 1;
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
            }

            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public IActionResult OnPostExportToJson(List<string> selectedColumns, bool isFiltered)
        {
            var query = ClassList.AsQueryable();

            if (isFiltered && !string.IsNullOrEmpty(FilterClassName))
            {
                query = query.Where(c => c.ClassName.ToLower().Contains(FilterClassName.ToLower()));
            }

            var dataToExport = query
                .Select(c => new ClassInformationTable
                {
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description,
                    HiddenId = c.Id
                }).ToList();

            var json = Utils.Instance.ExportToJson(dataToExport, selectedColumns);

            var fileName = $"ExportedData_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var absoluteFilePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(absoluteFilePath, json);

            var downloadUrl = $"/exports/{fileName}";
            return Redirect(downloadUrl);
        }

        // ✅ TÜM VERİYİ EXPORT EDER (CSV)
        public IActionResult OnPostExportAll(string SelectedColumns)
        {
            var selectedIndexes = SelectedColumns?.Split(',').Select(int.Parse).ToList() ?? new List<int> { 0, 1, 2 };
            var headerMap = new[] { "Class Name", "Student Count", "Description" };

            var lines = new List<string>
            {
                string.Join(",", selectedIndexes.Select(i => $"\"{headerMap[i]}\""))
            };

            foreach (var item in ClassList)
            {
                var row = new List<string>();
                if (selectedIndexes.Contains(0)) row.Add($"\"{item.ClassName}\"");
                if (selectedIndexes.Contains(1)) row.Add(item.StudentCount.ToString());
                if (selectedIndexes.Contains(2)) row.Add($"\"{item.Description}\"");
                lines.Add(string.Join(",", row));
            }

            var csvBytes = Encoding.UTF8.GetBytes(string.Join("\n", lines));
            return File(csvBytes, "text/csv", "exported_all_data.csv");
        }

        private static List<ClassInformationModel> GenerateSampleData()
        {
            var list = new List<ClassInformationModel>();
            for (int i = 1; i <= 100; i++)
            {
                list.Add(new ClassInformationModel
                {
                    Id = i,
                    ClassName = $"Class {i}",
                    StudentCount = 20 + (i % 30),
                    Description = $"Description for Class {i}"
                });
            }
            return list;
        }
    }
}
