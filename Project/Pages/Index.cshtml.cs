using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Week2.Models;
using Week2.Data;  // <-- DbContext dosyanın namespace'i
using Week2.Helpers;


//gpt: GPT, I sent you a task document that declares my work to do, can you help me?

//gpt: It is going well, now I need you to help me with the SMSS and SQL server conf. manager, how can I create an SQL and link it

//gpt: database link process giving me the error message on screenshot, what should I do?

//gpt: program.cs and appsettings.json update

//gpt: I need a soft delete process, delete should make active tag 0, and delete from screen

//gpt: class and utills namespaces are not seen by program, help me find out the reason.

//gpt: finally, I need to use http respone status codes, and check out them in Inspect menu.

namespace Week2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new();

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

        public async Task<IActionResult> OnGetAsync()
        {
            // Session ve cookie kontrolü
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
                return RedirectToPage("/Login");
            }

            // Verileri veritabanından çek
            var query = _context.Classes
                .Where(c => c.IsActive) // ❗ sadece aktif kayıtlar gelsin
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(FilterClassName))
            {
                query = query.Where(c => c.Name.ToLower().Contains(FilterClassName.ToLower()));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);
            var paged = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();

            FilteredList = paged.Select(c => new ClassInformationTable
            {
                ClassName = c.Name,
                StudentCount = c.PersonCount,
                Description = c.Description,
                HiddenId = c.Id
            }).ToList();

            if (EditId.HasValue)
            {
                var editingClass = await _context.Classes.FindAsync(EditId.Value);
                if (editingClass != null)
                {
                    EditingClass = new ClassInformationModel(editingClass.Id)
                    {
                        ClassName = editingClass.Name,
                        StudentCount = editingClass.PersonCount,
                        Description = editingClass.Description
                    };

                    NewClass = EditingClass;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            ModelState.Remove(nameof(FilterClassName));
            if (!ModelState.IsValid)
                return Page();

            var newClass = new Class
            {
                Name = NewClass.ClassName,
                PersonCount = NewClass.StudentCount,
                Description = NewClass.Description,
                IsActive = true
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            Response.StatusCode = 201;

            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                await _context.SaveChangesAsync();
            }
            Response.StatusCode = 201;
            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!EditId.HasValue)
                return RedirectToPage(new { FilterClassName, PageNumber });

            var item = await _context.Classes.FindAsync(EditId.Value);
            if (item != null && ModelState.IsValid)
            {
                item.Name = NewClass.ClassName;
                item.PersonCount = NewClass.StudentCount;
                item.Description = NewClass.Description;

                await _context.SaveChangesAsync();
            }

            Response.StatusCode = 200;

            return RedirectToPage(new { FilterClassName, PageNumber });
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");
            return RedirectToPage("/Login");
        }

        public async Task<IActionResult> OnPostExportAllAsync(string SelectedColumns)
        {
            var selectedIndexes = SelectedColumns?.Split(',').Select(int.Parse).ToList() ?? new List<int> { 0, 1, 2 };
            var columnNames = new[] { "Name", "PersonCount", "Description" };
            var selectedProperties = selectedIndexes.Select(i => columnNames[i]).ToList();

            var exportData = await _context.Classes.ToListAsync();

            var filteredData = exportData.Select(c => new ClassInformationTable
            {
                ClassName = c.Name,
                StudentCount = c.PersonCount,
                Description = c.Description,
                HiddenId = c.Id
            }).ToList();

            var json = Utils.Instance.ExportToJson(filteredData, selectedProperties);
            var fileName = $"ExportedAll_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            System.IO.File.WriteAllText(filePath, json);

            return Redirect($"/exports/{fileName}");
        }

        public async Task<IActionResult> OnPostExportFilteredAsync(string SelectedColumns, string FilterClassName)
        {
            var selectedIndexes = SelectedColumns?.Split(',').Select(int.Parse).ToList() ?? new List<int> { 0, 1, 2 };
            var columnNames = new[] { "Name", "PersonCount", "Description" };
            var selectedProperties = selectedIndexes.Select(i => columnNames[i]).ToList();

            var query = _context.Classes
                .Where(c => c.IsActive) // SADECE aktif olanlar gelsin
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterClassName))
            {
                query = query.Where(c => c.Name.ToLower().Contains(FilterClassName.ToLower()));
            }

            var exportData = await query.ToListAsync();

            var filteredData = exportData.Select(c => new ClassInformationTable
            {
                ClassName = c.Name,
                StudentCount = c.PersonCount,
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

        // İlk seferde JSON'dan veritabanına verileri taşıyacak method
        public async Task<IActionResult> OnPostMigrateJsonToDbAsync()
        {
            var list = GenerateSampleData();

            foreach (var item in list)
            {
                var cls = new Class
                {
                    Name = item.ClassName,
                    PersonCount = item.StudentCount,
                    Description = item.Description,
                    IsActive = true
                };

                _context.Classes.Add(cls);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // Sadece ilk veri yükleme için kullanılan method
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
