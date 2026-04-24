using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Web.Controllers;

public class ServicesController(TherapyDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(string category = null)
    {
        var query = dbContext.TherapyServices.Where(s => s.IsActive);
        
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(s => s.Category == category);
        }

        var services = await query.ToListAsync();
        
        ViewBag.Categories = await dbContext.TherapyServices
            .Where(s => s.IsActive)
            .Select(s => s.Category)
            .Distinct()
            .ToListAsync();
            
        ViewBag.SelectedCategory = category;

        return View(services);
    }

    public async Task<IActionResult> Details(int id)
    {
        var service = await dbContext.TherapyServices
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (service == null) return NotFound();

        return View(service);
    }
}
