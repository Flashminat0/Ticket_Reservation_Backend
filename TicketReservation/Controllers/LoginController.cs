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
    public async Task<IActionResult> Login([FromBody] AuthRequest? authRequest)
    {
        //validate the user
        if (authRequest == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Please provide the user credentials"
            };

            return BadRequest(apiFailedResponse);
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

            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = errorMessages
            };

            return BadRequest(apiFailedResponse);
        }

        var isUserExist = await _loginService.GetSingle(authRequest.Nic);

        if (isUserExist is null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User does not exists"
            };

            return BadRequest(apiFailedResponse);
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(authRequest.Password, isUserExist.Salt);

        var login = await _loginService.Login(authRequest.Nic, hashedPassword);

        if (login == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Invalid credentials"
            };

            return BadRequest(apiFailedResponse);
        }

        var newLogin = new Login()
        {
            Nic = login.Nic,
            Password = login.Password,
            IsActive = login.IsActive,
            Salt = login.Salt,
            Id = login.Id,
            LastLogin = DateTime.UtcNow,
        };

        await _loginService.Update(login.Nic, newLogin);

        AuthResponse authResponse = new AuthResponse()
        {
            Nic = login.Nic,
            IsActive = login.IsActive,
            IsAdmin = login.IsAdmin,
            LastLogin = login.LastLogin,
        };

        ApiResponse<AuthResponse> apiResponse = new ApiResponse<AuthResponse>()
        {
            Success = true,
            Message = "User logged in successfully",
            Data = authResponse
        };

        return Ok(apiResponse);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest? registerRequest)
    {
        //validate the user
        if (registerRequest == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Please provide the user credentials"
            };

            return BadRequest(apiFailedResponse);
        }

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

            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = errorMessages
            };

            return BadRequest(apiFailedResponse);
        }

        if (!registerRequest.Nic.ToLower().Contains('v'))
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Invalid NIC"
            };

            return BadRequest(apiFailedResponse);
        }

        if (registerRequest.Password.Length <= 7)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Password must be at least 8 characters"
            };

            return BadRequest(apiFailedResponse);
        }

        var login = await _loginService.GetSingle(registerRequest.Nic);

        if (login != null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User already exists"
            };

            return BadRequest(apiFailedResponse);
        }

        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password, salt);

        var newUser = new Login()
        {
            Nic = registerRequest.Nic,
            Password = hashedPassword,
            Salt = salt,
            LastLogin = DateTime.UtcNow,
            IsActive = true,
        };

        await _loginService.Register(newUser);

        AuthResponse authResponse = new AuthResponse()
        {
            Nic = registerRequest.Nic,
            IsActive = true,
            IsAdmin = false,
            LastLogin = DateTime.UtcNow,
        };

        ApiResponse<AuthResponse> apiResponse = new ApiResponse<AuthResponse>()
        {
            Success = true,
            Message = "User registered successfully",
            Data = authResponse
        };

        return Ok(apiResponse);
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateRequest? activateRequest)
    {
        if (activateRequest == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Please provide the user credentials"
            };

            return BadRequest(apiFailedResponse);
        }

        if (activateRequest.Nic == String.Empty)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Please provide the user NIC"
            };

            return BadRequest(apiFailedResponse);
        }

        var requester = await _loginService.GetSingle(activateRequest.RequestingNic);

        if (requester == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Requesting user does not exist."
            };

            return BadRequest(apiFailedResponse);
        }

        if (!requester.IsAdmin)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "You are not authorized to perform this action."
            };

            return BadRequest(apiFailedResponse);
        }


        var login = await _loginService.GetSingle(activateRequest.Nic);

        if (login == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User does not exists"
            };

            return BadRequest(apiFailedResponse);
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
        };

        ApiResponse<AuthResponse> apiResponse = new ApiResponse<AuthResponse>()
        {
            Success = true,
            Message = "User activated successfully",
            Data = authResponse
        };

        return Ok(apiResponse);
    }

    [HttpPost("token-login")]
    public async Task<IActionResult> LoginWithoutPassword([FromBody] AuthRequest? authRequest)
    {
        if (authRequest == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Please provide the user credentials"
            };

            return BadRequest(apiFailedResponse);
        }

        if (authRequest.Nic == "")
        {
            string errorMessages = "";

            if (authRequest.Nic == String.Empty)
            {
                errorMessages += "NIC is required. ";
            }

            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = errorMessages
            };

            return BadRequest(apiFailedResponse);
        }

        var isUserExist = await _loginService.GetSingle(authRequest.Nic);

        if (isUserExist is null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User does not exists"
            };

            return BadRequest(apiFailedResponse);
        }

        DateTime lastLogin = isUserExist.LastLogin;

        // check time difference
        int maximumInactiveTime = 3;
        TimeSpan timeDifference = DateTime.UtcNow - lastLogin;

        // _logger.LogInformation("Time difference: " + timeDifference.TotalMinutes);

        if (timeDifference.TotalMinutes > maximumInactiveTime)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "You have been logged out due to inactivity. Please login again."
            };

            return BadRequest(apiFailedResponse);
        }


        AuthResponse authResponse = new AuthResponse()
        {
            Nic = isUserExist.Nic,
            IsActive = isUserExist.IsActive,
            IsAdmin = isUserExist.IsAdmin,
            LastLogin = isUserExist.LastLogin,
        };

        string remainingTimeMessage = String.Empty;
        double remainingTime = (maximumInactiveTime - timeDifference.TotalMinutes);

        if (remainingTime < 1)
        {
            remainingTime = remainingTime * 60;
            string readableTime = remainingTime.ToString("0.##");
            remainingTimeMessage = readableTime + " seconds";
        }
        else
        {
            string readableTime = remainingTime.ToString("0.##");
            remainingTimeMessage = readableTime + " minutes";
        }

        ApiResponse<AuthResponse> apiResponse = new ApiResponse<AuthResponse>()
        {
            Success = true,
            Message = "User logged in successfully. You will be logged out in " + remainingTimeMessage,
            Data = authResponse
        };

        return Ok(apiResponse);
    }
}