using Microsoft.AspNetCore.Mvc;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserService _userService;

    public UserController(ILogger<UserController> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAll();

        return Ok(users);
    }


    [HttpGet("{nic}")]
    public async Task<IActionResult> GetUser(string nic)
    {
        var user = await _userService.GetSingle(nic);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        await _userService.Create(user);

        return CreatedAtAction(nameof(GetUser), new {nic = user.NIC}, user);
    }
}