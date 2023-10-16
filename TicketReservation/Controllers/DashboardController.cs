using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ReservationService _reservationService;
    private readonly TrainService _trainService;


    public DashboardController(UserService userService, ReservationService reservationService,
        TrainService trainService)
    {
        _userService = userService;
        _reservationService = reservationService;
        _trainService = trainService;
    }

    [Description("This endpoint is used to get all counts")]
    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var customers = await _userService.GetAllByType(UserTypeCl.Customer);
        var travelAgents = await _userService.GetAllByType(UserTypeCl.TravelAgent);
        
        var trains = await _trainService.GetAll();
        
        var reservations = await _reservationService.GetAll();
        
        ApiResponse<DashboardData> apiResponse = new ApiResponse<DashboardData>()
        {
            Success = true,
            Message = "Dashboard data retrieved successfully",
            Data = new DashboardData()
            {
                CustomerCount = customers.Count,
                TravelAgentCount = travelAgents.Count,
                TrainCount = trains.Count,
                ReservationCount = reservations.Count
            }
        };
        
        return Ok(apiResponse);
        
    }
}