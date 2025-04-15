using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Week2.Models;
using Week2.Helpers;
using System.Text;

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
            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            bool loginValid =
                !string.IsNullOrEmpty(sessionUsername) &&
                !string.IsNullOrEmpty(sessionToken) &&
                !string.IsNullOrEmpty(sessionId) &&
                sessionUsername == cookieUsername &&
                sessionToken == cookieToken &&
                sessionId == cookieSessionId;

            if (!loginValid)
            {
                Response.Redirect("/Login");
                return;
            }

            var query = ClassList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterClassName))
            {
                query = query.Where(c => c.ClassName.ToLower().Contains(FilterClassName.ToLower()));
            }

            TotalPages = (int)Math.Ceiling(query.Count() / (double)PageSize);
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

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");
            return RedirectToPage("/Login");
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
            {
                ClassList.Remove(item);
            }

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

        public IActionResult OnPostExportAll(string SelectedColumns)
        {
            var selectedIndexes = SelectedColumns?.Split(',').Select(int.Parse).ToList() ?? new List<int> { 0, 1, 2 };
            var columnNames = new[] { "ClassName", "StudentCount", "Description" };
            var selectedProperties = selectedIndexes.Select(i => columnNames[i]).ToList();

            var exportData = ClassList.Select(c => new ClassInformationTable
            {
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description,
                HiddenId = c.Id
            }).ToList();

            var json = Utils.Instance.ExportToJson(exportData, selectedProperties);
            var fileName = $"ExportedAll_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            System.IO.File.WriteAllText(filePath, json);

            return Redirect($"/exports/{fileName}");
        }

        public IActionResult OnPostExportFiltered(string SelectedColumns, string FilterClassName)
        {
            var selectedIndexes = SelectedColumns?.Split(',').Select(int.Parse).ToList() ?? new List<int> { 0, 1, 2 };
            var columnNames = new[] { "ClassName", "StudentCount", "Description" };
            var selectedProperties = selectedIndexes.Select(i => columnNames[i]).ToList();

            var query = ClassList.AsQueryable();
            if (!string.IsNullOrWhiteSpace(FilterClassName))
            {
                query = query.Where(c => c.ClassName.ToLower().Contains(FilterClassName.ToLower()));
            }

            var filteredData = query.Select(c => new ClassInformationTable
            {
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description,
                HiddenId = c.Id
            }).ToList();

            var json = Utils.Instance.ExportToJson(filteredData, selectedProperties);
            var fileName = $"ExportedFiltered_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            System.IO.File.WriteAllText(filePath, json);

            return Redirect($"/exports/{fileName}");
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
