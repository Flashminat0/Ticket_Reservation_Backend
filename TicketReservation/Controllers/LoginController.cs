using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers;

[ApiController]
[Route("api/auth")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly LoginService _loginService;

    public LoginController(ILogger<LoginController> logger, LoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        //validate the user
        if (loginRequest == null)
        {
            return BadRequest();
        }

        if (loginRequest.Nic == null || loginRequest.Password == null)
        {
            string errorMessages = "";

            if (loginRequest.Nic == null)
            {
                errorMessages += "NIC is required. ";
            }

            if (loginRequest.Password == null)
            {
                errorMessages += "Password is required. ";
            }

            return BadRequest(errorMessages);
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password, loginRequest.Nic);

        var login = await _loginService.Login(loginRequest.Nic, hashedPassword);

        if (login == null)
        {
            return NotFound();
        }

        return Ok(login);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest registerRequest)
    {
        //validate the user
        if (registerRequest == null)
        {
            return BadRequest();
        }

        _logger.LogInformation(registerRequest.ToJson());

        if (registerRequest.Nic == String.Empty || registerRequest.Password == String.Empty)
        {
            string errorMessages = "";

            if (registerRequest.Nic == String.Empty)
            {
                errorMessages += "NIC is required. ";
            }

            if (registerRequest.Password == String.Empty)
            {
                errorMessages += "Password is required. ";
            }

            return BadRequest(errorMessages);
        }

        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password, salt);

        var login = await _loginService.GetSingle(registerRequest.Nic);
        
        _logger.LogInformation(login.ToJson());

        if (login != null)
        {
            return BadRequest("User already exists");
        }

        await _loginService.Register(new Login
        {
            Nic = registerRequest.Nic,
            Password = hashedPassword,
            Salt = salt,
        });

        return Ok("User registered successfully");
    }
}