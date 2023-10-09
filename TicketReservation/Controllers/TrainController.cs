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
        private readonly UserService _userService;

        public readonly List<string> districts = new List<string>
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

        public TrainController(ILogger<TrainController> logger, TrainService trainService, UserService userService)
        {
            _logger = logger;
            _trainService = trainService;
            _userService = userService;
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

        [Description("This endpoint is used to get all districts")]
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts()
        {
            ApiResponse<IEnumerable<string>> apiResponse = new ApiResponse<IEnumerable<string>>()
            {
                Success = true,
                Message = "Districts retrieved successfully",
                Data = districts
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to get a single train by its ID")]
        [HttpGet("train/{id}")]
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

        [Description("This endpoint is used to get a single train by its owner NIC")]
        [HttpGet("owner/{nic}")]
        public async Task<IActionResult> GetTrainByOwner(string nic)
        {
            var trains = await _trainService.GetByOwner(nic);

            if (trains == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            ApiResponse<IEnumerable<Train>> apiResponse = new ApiResponse<IEnumerable<Train>>()
            {
                Success = true,
                Message = "Train retrieved successfully",
                Data = trains
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

            foreach (string trainDistrict in train.Districts)
            {
                if (!districts.Contains(trainDistrict))
                {
                    ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                    {
                        Success = false,
                        Message = "Districts contain invalid district"
                    };

                    return BadRequest(apiFailedResponse);
                }
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

            if (train.StartTime > train.EndTime)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Start time must be before end time"
                };

                return BadRequest(apiFailedResponse);
            }

            if (train.Price < 0)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Price cannot be negative"
                };

                return BadRequest(apiFailedResponse);
            }

            if (train.Seats < 0)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Seats cannot be negative"
                };

                return BadRequest(apiFailedResponse);
            }

            if (!train.OwnerNic.ToLower().Contains('v'))
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner NIC must be 10 characters long"
                };

                return BadRequest(apiFailedResponse);
            }

            var owner = await _userService.GetSingle(train.OwnerNic);

            if (owner == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner NIC is invalid"
                };

                return BadRequest(apiFailedResponse);
            }

            _logger.LogInformation(owner.UserType);

            if (owner.UserType.ToLower() == UserTypeCl.Customer.ToLower())
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner is not a Travel Agent nor Backoffice Staff"
                };

                return BadRequest(apiFailedResponse);
            }


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
                OwnerNic = train.OwnerNic
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