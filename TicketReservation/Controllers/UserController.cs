using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserService _userService;
    private readonly LoginService _loginService;

    public UserController(ILogger<UserController> logger, UserService userService, LoginService loginService)
    {
        _logger = logger;
        _userService = userService;
        _loginService = loginService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAll();

        ApiResponse<IEnumerable<User>> apiResponse = new ApiResponse<IEnumerable<User>>()
        {
            Success = true,
            Message = "Users retrieved successfully",
            Data = users
        };

        return Ok(apiResponse);
    }


    [HttpGet("{nic}")]
    public async Task<IActionResult> GetUser(string nic)
    {
        var user = await _userService.GetSingle(nic);

        if (user == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User not found"
            };

            return NotFound(apiFailedResponse);
        }

        ApiResponse<User> apiResponse = new ApiResponse<User>()
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = user
        };

        return Ok(apiResponse);
    }

    [HttpGet("type/{userType}")]
    public async Task<IActionResult> GetUsersByType(string userType)
    {
        if (userType == String.Empty)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User type is required"
            };

            return BadRequest(apiFailedResponse);
        }

        string userTypeToCheck = UserTypeCl.Customer;

        if (userType != String.Empty)
        {
            if (userType.ToLower() == UserTypeCl.Backoffice.ToLower())
            {
                userTypeToCheck = UserTypeCl.Backoffice;
            }
            else if (userType.ToLower() == UserTypeCl.TravelAgent.ToLower())
            {
                userTypeToCheck = UserTypeCl.TravelAgent;
            }
            else if (userType.ToLower() == UserTypeCl.Customer.ToLower())
            {
                userTypeToCheck = UserTypeCl.Customer;
            }
            else
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message =
                        "This user type is not supported. Supported user types are: [Backoffice], [Travel Agent], [Customer]"
                };

                return BadRequest(apiFailedResponse);
            }
        }

        var users = await _userService.GetAllByType(userTypeToCheck);

        if (users.Count == 0)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Users not found"
            };

            return NotFound(apiFailedResponse);
        }

        ApiResponse<IEnumerable<User>> apiResponse = new ApiResponse<IEnumerable<User>>()
        {
            Success = true,
            Message = "Users retrieved successfully",
            Data = users
        };

        return Ok(apiResponse);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest user)
    {
        //validate the user
        if (user == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User not found"
            };

            return BadRequest(apiFailedResponse);
        }

        var existingUser = await _userService.GetSingle(user.Nic);

        if (existingUser != null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User already exists. Please use update endpoint to update the user"
            };

            return BadRequest(apiFailedResponse);
        }

        if (user.Nic == String.Empty || user.Name == String.Empty || user.Age == 0 || user.Age < 0 ||
            user.UserType == String.Empty || user.UserGender == String.Empty)
        {
            string errorMessages = "";

            if (user.Nic == String.Empty)
            {
                errorMessages += "NIC is required. ";
            }

            if (user.Name == String.Empty)
            {
                errorMessages += "Name is required. ";
            }

            if (user.Age == 0)
            {
                errorMessages += "Age is required. ";
            }

            if (user.UserType == String.Empty)
            {
                errorMessages += "User type is required. ";
            }

            if (user.UserGender == String.Empty)
            {
                errorMessages += "User Gender is required. ";
            }

            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = errorMessages
            };

            return BadRequest(apiFailedResponse);
        }

        if (!user.Nic.ToLower().Contains('v'))
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Wrong NIC format."
            };

            return BadRequest(apiFailedResponse);
        }


        var userToCheck = await _loginService.GetSingle(user.Nic);

        if (userToCheck == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User not Registered yet"
            };

            return NotFound(apiFailedResponse);
        }

        if (userToCheck.IsActive == false)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User is not active"
            };

            return BadRequest(apiFailedResponse);
        }

        var userType = UserTypeCl.Customer;


        if (user.UserType != String.Empty)
        {
            if (user.UserType.ToLower() == UserTypeCl.Backoffice.ToLower())
            {
                userType = UserTypeCl.Backoffice;
            }
            else if (user.UserType.ToLower() == UserTypeCl.TravelAgent.ToLower())
            {
                userType = UserTypeCl.TravelAgent;
            }
            else if (user.UserType.ToLower() == UserTypeCl.Customer.ToLower())
            {
                userType = UserTypeCl.Customer;
            }
        }

        string userGender = UserGenderCl.Male;

        if (user.UserGender != String.Empty)
        {
            if (user.UserGender.ToLower() == UserGenderCl.Male.ToLower())
            {
                userGender = UserGenderCl.Male;
            }
            else
            {
                userGender = UserGenderCl.Female;
            }
        }


        User newUser = new User
        {
            Nic = user.Nic,
            Name = user.Name,
            Age = user.Age,
            UserType = userType,
            UserGender = userGender
        };


        await _userService.Create(newUser);

        ApiResponse<User> apiResponse = new ApiResponse<User>()
        {
            Success = true,
            Message = "User created successfully",
            Data = newUser
        };

        return Ok(apiResponse);
    }


    [HttpPut("{nic}")]
    public async Task<IActionResult> UpdateUser(string nic, [FromBody] EditUserRequest user)
    {
        //validate the user
        if (user == null)
        {
            return BadRequest();
        }

        if (nic != String.Empty && !nic.Contains('v'))
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "Wrong NIC format."
            };

            return BadRequest(apiFailedResponse);
        }

        var userToUpdate = await _userService.GetSingle(nic);

        if (userToUpdate == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User not found"
            };

            return NotFound(apiFailedResponse);
        }

        User updatedUser = new User
        {
            Id = userToUpdate.Id,
            Nic = userToUpdate.Nic,
            Name = user.Name ?? userToUpdate.Name,
            Age = user.Age ?? userToUpdate.Age,
            UserType = user.UserType ?? userToUpdate.UserType,
            UserGender = user.UserGender ?? userToUpdate.UserGender,
        };

        await _userService.Update(nic, updatedUser);

        ApiResponse<User> apiResponse = new ApiResponse<User>()
        {
            Success = true,
            Message = "User updated successfully",
            Data = updatedUser
        };

        return Ok(apiResponse);
    }


    [HttpDelete("{nic}")]
    public async Task<IActionResult> DeleteUser(string nic)
    {
        // var adminUser = await _userService.GetSingle(requestUser.Nic);
        //
        // if (adminUser == null)
        // {
        //     ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
        //     {
        //         Success = false,
        //         Message = "Please Provide Admin Creds. Or no Admin found with the given NIC"
        //     };
        //
        //     return BadRequest(apiFailedResponse);
        // }
        //
        // if (!adminUser.UserType.ToLower().Equals(UserTypeCl.Customer.ToLower()))
        // {
        //     ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
        //     {
        //         Success = false,
        //         Message = "You are not authorized to perform this action"
        //     };
        //
        //     return BadRequest(apiFailedResponse);
        // }


        var user = await _userService.GetSingle(nic);

        if (user == null)
        {
            ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
            {
                Success = false,
                Message = "User not found"
            };

            return NotFound(apiFailedResponse);
        }

        await _userService.Remove(nic);
        await _loginService.Remove(nic);

        ApiResponse<String> apiResponse = new ApiResponse<String>()
        {
            Success = true,
            Message = "User deleted successfully",
            Data = "GET FOK"
        };

        return Ok(apiResponse);
    }
}