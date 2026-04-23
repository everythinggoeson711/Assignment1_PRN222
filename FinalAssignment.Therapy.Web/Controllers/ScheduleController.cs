using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalAssignment.Therapy.Web.Controllers;

public class ScheduleController(IScheduleService scheduleService, ITherapyLookupService lookupService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Calendar(int? therapistId, CancellationToken cancellationToken)
    {
        var therapists = await lookupService.GetActiveTherapistsAsync(cancellationToken);
        var services = await lookupService.GetActiveServicesAsync(cancellationToken);
        
        ViewBag.Therapists = therapists;
        ViewBag.Services = services;
        
        if (therapistId.HasValue)
        {
            var startDate = DateTime.Today;
            ViewBag.SelectedTherapistId = therapistId.Value;
            ViewBag.SelectedDate = startDate;
        }
        
        return View();
    }

    [HttpGet("Schedule/GetAvailableSlots")]
    public async Task<IActionResult> GetAvailableSlots(int therapistId, int serviceId, DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var services = await lookupService.GetActiveServicesAsync(cancellationToken);
            var service = services.FirstOrDefault(s => s.Id == serviceId);
            
            if (service == null)
            {
                return Json(new { success = false, error = "Service not found", slots = new object[0] });
            }
            
            var availableSlots = await scheduleService.GetAvailableSlotsAsync(therapistId, date, service.DurationMinutes, cancellationToken);
            
            return Json(new { 
                success = true,
                slots = availableSlots.Select(slot => new {
                    startTime = slot.ToString("HH:mm"),
                    endTime = slot.AddMinutes(service.DurationMinutes).ToString("HH:mm"),
                    dateTime = slot.ToString("yyyy-MM-ddTHH:mm:ss")
                })
            });
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false,
                error = ex.Message,
                slots = new object[0]
            });
        }
    }
}