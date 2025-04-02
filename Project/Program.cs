var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// 🔽 Session servisini ekle
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔽 Session'ı kullan
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();

app.Run();
