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
            "ampara",
            "anuradhapura",
            "badulla",
            "batticaloa",
            "colombo",
            "galle",
            "gampaha",
            "hambantota",
            "jaffna",
            "kalutara",
            "kandy",
            "kegalle",
            "kilinochchi",
            "kurunegala",
            "mannar",
            "matale",
            "matara",
            "monaragala",
            "mullaitivu",
            "nuwara eliya",
            "polonnaruwa",
            "puttalam",
            "ratnapura",
            "trincomalee",
            "vavuniya"
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
        public async Task<IActionResult> CreateTrain(CreateTrainRequest train)
        {
            if (!districts.Contains(train.StartStation) || !districts.Contains(train.EndStation))
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Start station or end station is invalid"
                };

                return BadRequest(apiFailedResponse);
            }

            if (train.StartStation == train.EndStation)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Start station and end station cannot be the same"
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

            string trainNumber = train.StartStation.Substring(0, 1) + train.EndStation.Substring(0, 1) + "-" +
                                 train.StartTime.ToString("HHmm") + "-" + train.EndTime.ToString("HHmm");

            Train createdTrain = new Train()
            {
                TrainName = train.TrainName,
                TrainType = train.TrainType,
                StartStation = train.StartStation,
                EndStation = train.EndStation,
                StartTime = train.StartTime,
                EndTime = train.EndTime,
                Price = train.Price,
                Districts = train.Districts,
                Seats = train.Seats,
                TrainNumber = trainNumber
            };

            await _trainService.Create(createdTrain);

            ApiResponse<Train> apiResponse = new ApiResponse<Train>()
            {
                Success = true,
                Message = "Train created successfully",
                Data = createdTrain
            };

            return Ok(apiResponse);
        }
    }
}