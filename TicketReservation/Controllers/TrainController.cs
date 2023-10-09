using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainController : ControllerBase
    {
        private readonly ILogger<TrainController> _logger;
        private readonly TrainService _trainService;

        private readonly List<string> districts = new List<string>
        {
            "Ampara",
            "Anuradhapura",
            "Badulla",
            "Batticaloa",
            "Colombo",
            "Galle",
            "Gampaha",
            "Hambantota",
            "Jaffna",
            "Kalutara",
            "Kandy",
            "Kegalle",
            "Kilinochchi",
            "Kurunegala",
            "Mannar",
            "Matale",
            "Matara",
            "Monaragala",
            "Mullaitivu",
            "Nuwara Eliya",
            "Polonnaruwa",
            "Puttalam",
            "Ratnapura",
            "Trincomalee",
            "Vavuniya"
        };

        public TrainController(ILogger<TrainController> logger, TrainService trainService)
        {
            _logger = logger;
            _trainService = trainService;
        }

        [Description("This endpoint is used to get all trains")]
        [HttpGet]
        public async Task<IActionResult> GetTrains()
        {
            var trains = await _trainService.GetAll();

            ApiResponse<IEnumerable<Train>> apiResponse = new ApiResponse<IEnumerable<Train>>()
            {
                Success = true,
                Message = "Trains retrieved successfully",
                Data = trains
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to get a single train by its ID")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrain(string id)
        {
            var train = await _trainService.GetSingle(id);

            if (train == null)
            {
                return NotFound();
            }

            ApiResponse<Train> apiResponse = new ApiResponse<Train>()
            {
                Success = true,
                Message = "Train retrieved successfully",
                Data = train
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to create a new train")]
        [HttpPost]
        public async Task<IActionResult> CreateTrain(Train train)
        {
            if (train.StartStation == train.EndStation)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Start station and end station cannot be the same"
                };

                return BadRequest(apiFailedResponse);
            }

            if (!districts.Contains(train.StartStation) || !districts.Contains(train.EndStation))
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Start station or end station is invalid"
                };

                return BadRequest(apiFailedResponse);
            }

            if (train.Districts.Count < 2)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train must have at least 2 stations"
                };

                return BadRequest(apiFailedResponse);
            }

            await _trainService.Create(train);

            ApiResponse<Train> apiResponse = new ApiResponse<Train>()
            {
                Success = true,
                Message = "Train created successfully",
                Data = train
            };

            return Ok(apiResponse);
        }
    }
}