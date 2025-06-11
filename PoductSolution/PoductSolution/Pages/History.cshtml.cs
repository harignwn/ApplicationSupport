using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


public class HistoryModel : PageModel
{
    private readonly AppDbContext _context;
    public HistoryModel(AppDbContext context) => _context = context;
//    AppDbContext adalah context Entity Framework Core untuk mengakses database.

//Disuntikkan melalui konstruktor (Dependency Injection) agar bisa digunakan untuk query data Planning.


    public List<Planning> AllPlans { get; set; }

    public async Task OnGetAsync()
    {
        AllPlans = await _context.Plannings.OrderByDescending(x => x.TanggalInput).ToListAsync();
    }
}
