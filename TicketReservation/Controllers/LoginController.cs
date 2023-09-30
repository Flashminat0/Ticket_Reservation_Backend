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
    public async Task<IActionResult> Login([FromBody] AuthRequest authRequest)
    {
        //validate the user
        if (authRequest == null)
        {
            return BadRequest();
        }

        if (authRequest.Nic == "" || authRequest.Password == "")
        {
            string errorMessages = "";

            if (authRequest.Nic == String.Empty)
            {
                errorMessages += "NIC is required. ";
            }

            if (authRequest.Password == String.Empty)
            {
                errorMessages += "Password is required. ";
            }

            return BadRequest(errorMessages);
        }

        var isUserExist = await _loginService.GetSingle(authRequest.Nic);

        // _logger.LogInformation(isUserExist.ToJson());

        if (isUserExist is null)
        {
            return BadRequest("User does not exists");
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(authRequest.Password, isUserExist.Salt);

        var login = await _loginService.Login(authRequest.Nic, hashedPassword);

        if (login is null)
        {
            return BadRequest("Incorrect Credentials");
        }

        var newLogin = new Login()
        {
            Nic = login.Nic,
            Password = login.Password,
            IsActive = login.IsActive,
            Salt = login.Salt,
            Id = login.Id,
            LastLogin = DateTime.Now,
        };

        await _loginService.Update(login.Nic, newLogin);

        AuthResponse authResponse = new AuthResponse()
        {
            Nic = login.Nic,
            IsActive = login.IsActive,
            IsAdmin = login.IsAdmin,
            LastLogin = login.LastLogin,
            Message = "User logged in successfully"
        };

        return Ok(authResponse);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest registerRequest)
    {
        //validate the user
        if (registerRequest == null)
        {
            return BadRequest();
        }

        // _logger.LogInformation(registerRequest.ToJson());

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

        // _logger.LogInformation(login.ToJson());

        if (login != null)
        {
            return BadRequest("User already exists");
        }

        await _loginService.Register(new Login
        {
            Nic = registerRequest.Nic,
            Password = hashedPassword,
            Salt = salt,
            LastLogin = DateTime.Now,
            IsActive = true,
        });

        AuthResponse authResponse = new AuthResponse()
        {
            Nic = login.Nic,
            IsActive = login.IsActive,
            IsAdmin = login.IsAdmin,
            LastLogin = login.LastLogin,
            Message = "User registered successfully"
        };

        return Ok(authResponse);
    }


    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateRequest activateRequest)
    {
        if (activateRequest == null)
        {
            return BadRequest();
        }

        if (activateRequest.Nic == String.Empty)
        {
            return BadRequest("NIC is required");
        }

        var requester = await _loginService.GetSingle(activateRequest.RequestingNic);

        if (!requester.IsAdmin)
        {
            return BadRequest("You are not authorized to perform this action");
        }


        var login = await _loginService.GetSingle(activateRequest.Nic);

        if (login == null)
        {
            return BadRequest("User does not exists");
        }

        var newLogin = new Login()
        {
            Nic = login.Nic,
            Password = login.Password,
            IsActive = activateRequest.IsActive,
            IsAdmin = activateRequest.IsAdmin,
            Salt = login.Salt,
            Id = login.Id,
            LastLogin = login.LastLogin,
        };

        await _loginService.Update(login.Nic, newLogin);

        AuthResponse authResponse = new AuthResponse()
        {
            Nic = login.Nic,
            IsActive = newLogin.IsActive,
            IsAdmin = newLogin.IsAdmin,
            LastLogin = login.LastLogin,
            Message = "User activated successfully"
        };


        return Ok(authResponse);
    }
}