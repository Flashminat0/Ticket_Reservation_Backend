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
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest user)
    {
        //validate the user
        if (user == null)
        {
            return BadRequest();
        }

        if (user.Nic == String.Empty || user.Name == String.Empty || user.Age == 0 || user.Age < 0 || user.UserType == String.Empty)
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

            return BadRequest(errorMessages);
        }

        if (!user.Nic.ToLower().Contains('v'))
        {
            return BadRequest("Wrong NIC format.");
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
        };


        await _userService.Create(newUser);

        return CreatedAtAction(nameof(GetUser), new { nic = user.Nic }, user);
    }


    [HttpPut("{nic}")]
    public async Task<IActionResult> UpdateUser(string nic, [FromBody] EditUserRequest user)
    {
        //validate the user
        if (user == null)
        {
            return BadRequest();
        }

        if (user.Name == String.Empty || user.Age == 0 || user.Age < 0)
        {
            string errorMessages = "";


            if (user.Name == String.Empty)
            {
                errorMessages += "Name is required. ";
            }

            if (user.Age <= 0)
            {
                errorMessages += "Age is required. ";
            }

            return BadRequest(errorMessages);
        }

        if (!nic.Contains('v'))
        {
            return BadRequest("Wrong NIC format.");
        }

        var userToUpdate = await _userService.GetSingle(nic);

        if (userToUpdate == null)
        {
            return NotFound();
        }

        User newUser = new User
        {
            Id = userToUpdate.Id,
            Nic = userToUpdate.Nic,
            Name = user.Name ?? userToUpdate.Name,
            Age = user.Age ?? userToUpdate.Age,
        };

        await _userService.Update(nic, newUser);


        return CreatedAtAction(nameof(GetUser), new { nic = nic }, user);
    }


    [HttpDelete("{nic}")]
    public async Task<IActionResult> DeleteUser(string nic)
    {
        var user = await _userService.GetSingle(nic);

        if (user == null)
        {
            return NotFound();
        }

        await _userService.Remove(nic);

        return NoContent();
    }
}