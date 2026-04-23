using System.Diagnostics;
using FinalAssignment.Therapy.Application.Services;
using Microsoft.AspNetCore.Mvc;
using FinalAssignment.Therapy.Web.Models;

namespace FinalAssignment.Therapy.Web.Controllers;

public class HomeController(ITherapyLookupService therapyLookupService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = new LandingPageViewModel
        {
            Therapists = await therapyLookupService.GetActiveTherapistsAsync(cancellationToken),
            Services = await therapyLookupService.GetActiveServicesAsync(cancellationToken)
        };

        return View(viewModel);
    }

    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
