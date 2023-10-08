using System.ComponentModel;
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

    [Description("This endpoint is used to login to the system.")]
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

            return Unauthorized(apiFailedResponse);
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

            return Unauthorized(apiFailedResponse);
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

    [Description("This endpoint is used to register a new user.")]
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

    [Description("This endpoint is used to activate or deactivate a user.")]
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

    [Description("This endpoint is used to login to the system without a password.")]
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
        int maximumInactiveTime = 15;
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

        // Security Flaw
        // var newLogin = new Login()
        // {
        //     Nic = isUserExist.Nic,
        //     Password = isUserExist.Password,
        //     IsActive = isUserExist.IsActive,
        //     IsAdmin = isUserExist.IsAdmin,
        //     Salt = isUserExist.Salt,
        //     Id = isUserExist.Id,
        //     LastLogin = DateTime.UtcNow,
        // };
        //
        // await _loginService.Update(isUserExist.Nic, newLogin);


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
            string seconds = $".{remainingTime.ToString("N").Split('.')[1]}";
            string readableSeconds = (Convert.ToDecimal(seconds) * 60).ToString("0.##").Split('.')[0];

            if (readableSeconds == "0")
            {
                remainingTimeMessage = "";
            }
            else if (readableSeconds == "1")
            {
                remainingTimeMessage = "1 second";
            }
            else
            {
                remainingTimeMessage = readableSeconds + " seconds";
            }
        }
        else
        {
            string minutes = remainingTime.ToString("N").Split('.')[0];
            string seconds = $".{remainingTime.ToString("N").Split('.')[1]}";


            string readableSeconds = (Convert.ToDecimal(seconds) * 60).ToString("0.##").Split('.')[0];

            if (readableSeconds == "0")
            {
                if (minutes == "1")
                {
                    remainingTimeMessage = minutes + " minute";
                }
                else
                {
                    remainingTimeMessage = minutes + " minutes";
                }
            }
            else
            {
                if (minutes == "1")
                {
                    remainingTimeMessage = minutes + " minute " + readableSeconds + " seconds";
                }
                else
                {
                    remainingTimeMessage = minutes + " minutes " + readableSeconds + " seconds";
                }
            }
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