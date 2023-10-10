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
        private readonly ReservationService _reservationService;

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

        public TrainController(ILogger<TrainController> logger, TrainService trainService, UserService userService,
            ReservationService reservationService)
        {
            _logger = logger;
            _trainService = trainService;
            _userService = userService;
            _reservationService = reservationService;
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
                OwnerNic = train.OwnerNic,
                IsActive = train.IsActive
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

        [Description("This endpoint is used to edit a train")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditTrain(string id ,EditTrainRequest? train)
        {
            if (train == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train is null"
                };

                return BadRequest(apiFailedResponse);
            }

            if (train.Id == null || train.Id == "")
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train ID is null"
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

            if (!train.EditingNic.ToLower().Contains('v'))
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner NIC must be 10 characters long"
                };

                return BadRequest(apiFailedResponse);
            }

            var editor = await _userService.GetSingle(train.EditingNic);

            if (editor == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner NIC is invalid"
                };

                return BadRequest(apiFailedResponse);
            }

            // _logger.LogInformation(editor.UserType);

            if (editor.UserType.ToLower() == UserTypeCl.Customer.ToLower())
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner is not a Travel Agent nor Backoffice Staff"
                };

                return BadRequest(apiFailedResponse);
            }

            var trainToEdit = await _trainService.GetSingle(train.Id);


            if (trainToEdit == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            if (editor.UserType.ToLower() != UserTypeCl.Backoffice.ToLower())
            {
                if (trainToEdit.OwnerNic != train.EditingNic)
                {
                    ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                    {
                        Success = false,
                        Message = "You are not the owner of this train nor a backoffice staff"
                    };

                    return BadRequest(apiFailedResponse);
                }
            }


            Train editedTrain = new Train()
            {
                Id = id,
                TrainName = train.TrainName,
                TrainType = train.TrainType,
                StartStation = train.StartStation,
                EndStation = train.EndStation,
                StartTime = train.StartTime,
                EndTime = train.EndTime,
                Price = train.Price,
                Districts = train.Districts,
                Seats = train.Seats,
                IsActive = train.IsActive,
                OwnerNic = trainToEdit.OwnerNic
            };

            _logger.LogInformation(editedTrain.Id);
            
            await _trainService.Update(id, editedTrain);

            ApiResponse<Train> apiResponse = new ApiResponse<Train>()
            {
                Success = true,
                Message = "Train Updated successfully",
                Data = editedTrain
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to delete a train")]
        [HttpDelete("{id}/{nic}")]
        public async Task<IActionResult> DeleteTrain(string id, string nic)
        {
            var train = await _trainService.GetSingle(id);

            if (train == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            var deletor = await _userService.GetSingle(nic);

            if (deletor == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner NIC is invalid"
                };

                return BadRequest(apiFailedResponse);
            }

            // _logger.LogInformation(editor.UserType);

            if (deletor.UserType.ToLower() == UserTypeCl.Customer.ToLower())
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Owner is not a Travel Agent nor Backoffice Staff"
                };

                return BadRequest(apiFailedResponse);
            }


            if (deletor.UserType.ToLower() != UserTypeCl.Backoffice.ToLower())
            {
                if (train.OwnerNic != nic)
                {
                    ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                    {
                        Success = false,
                        Message = "You are not the owner of this train nor a backoffice staff"
                    };

                    return BadRequest(apiFailedResponse);
                }
            }

            var reservations = await _reservationService.GetByTrainID(id);

            if (reservations.Count > 0)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train has reservations"
                };

                return BadRequest(apiFailedResponse);
            }

            await _trainService.Remove(id);

            ApiResponse<Train> apiResponse = new ApiResponse<Train>()
            {
                Success = true,
                Message = "Train deleted successfully",
                Data = train
            };

            return Ok(apiResponse);
        }
    }
}