using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using TicketReservation.Models;
using TicketReservation.Services;

namespace TicketReservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        // private readonly ILogger<TrainController> _logger;
        private readonly ReservationService _reservationService;
        private readonly TrainService _trainService;
        private readonly UserService _userService;


        public ReservationController(ReservationService reservationService,
            TrainService trainService, UserService userService)
        {
            // _logger = logger;
            _reservationService = reservationService;
            _trainService = trainService;
            _userService = userService;
        }


        [Description("This endpoint is used to get all the reservations in the database.")]
        [HttpGet]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _reservationService.GetAll();

            ApiResponse<IEnumerable<Reservation>> apiResponse = new ApiResponse<IEnumerable<Reservation>>()
            {
                Success = true,
                Message = "Reservations retrieved successfully",
                Data = reservations
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to get a reservation by the user NIC.")]
        [HttpGet("nic/{nic}")]
        public async Task<IActionResult> GetReservationByNic(string nic)
        {
            var reservations = await _reservationService.GetByNic(nic);

            if (reservations.Count == 0)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Reservation not found"
                };

                return NotFound(apiFailedResponse);
            }

            ApiResponse<IEnumerable<Reservation>> apiResponse = new ApiResponse<IEnumerable<Reservation>>()
            {
                Success = true,
                Message = "Reservation retrieved successfully",
                Data = reservations
            };

            return Ok(apiResponse);
        }

        [Description("This endpoint is used to get a reservation by the train ID.")]
        [HttpGet("train/{id}")]
        public async Task<IActionResult> GetReservationByTrainId(string id)
        {
            var reservations = await _reservationService.GetByTrainID(id);

            if (reservations.Count == 0)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Reservation not found"
                };

                return NotFound(apiFailedResponse);
            }

            ApiResponse<IEnumerable<Reservation>> apiResponse = new ApiResponse<IEnumerable<Reservation>>()
            {
                Success = true,
                Message = "Reservation retrieved successfully",
                Data = reservations
            };

            return Ok(apiResponse);
        }

        //Create
        [Description("This endpoint is used to create a reservation.")]
        [HttpPost]
        public async Task<IActionResult> CreateReservation(Reservation reservation)
        {
            var user = await _userService.GetSingle(reservation.UserNic);
            if (user == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "User not found"
                };

                return NotFound(apiFailedResponse);
            }

            var train = await _trainService.GetSingle(reservation.TrainId);

            if (train == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            if (train.Seats < reservation.Seats)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Not enough seats"
                };

                return BadRequest(apiFailedResponse);
            }

            train.Seats -= reservation.Seats;

            await _trainService.Update(train.Id, train);
            await _reservationService.Create(reservation);

            ApiResponse<Reservation> apiResponse = new ApiResponse<Reservation>()
            {
                Success = true,
                Message = "Reservation created successfully",
                Data = reservation
            };

            return Ok(apiResponse);
        }

        //Edit
        [Description("This endpoint is used to edit a reservation.")]
        [HttpPut("{reservationID}")]
        public async Task<IActionResult> EditReservation(string reservationID, Reservation reservationIn)
        {
            var reservations = await _reservationService.GetByReservationID(reservationID);

            if (reservations == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Reservation not found"
                };

                return NotFound(apiFailedResponse);
            }

            var train = await _trainService.GetSingle(reservationIn.TrainId);

            if (train == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            int seats = reservations.Seats + train.Seats;

            if (seats < reservationIn.Seats)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Not enough seats"
                };

                return BadRequest(apiFailedResponse);
            }

            train.Seats -= reservationIn.Seats;

            await _trainService.Update(train.Id, train);
            await _reservationService.Update(reservationIn.UserNic, reservationIn);

            ApiResponse<Reservation> apiResponse = new ApiResponse<Reservation>()
            {
                Success = true,
                Message = "Reservation updated successfully",
                Data = reservationIn
            };

            return Ok(apiResponse);
        }

        //Delete
        [Description("This endpoint is used to delete a reservation.")]
        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(string reservationId)
        {
            var reservation = await _reservationService.GetByReservationID(reservationId);

            if (reservation == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Reservation not found"
                };

                return NotFound(apiFailedResponse);
            }

            var train = await _trainService.GetSingle(reservation.TrainId);

            if (train == null)
            {
                ApiFailedResponse apiFailedResponse = new ApiFailedResponse()
                {
                    Success = false,
                    Message = "Train not found"
                };

                return NotFound(apiFailedResponse);
            }

            train.Seats += reservation.Seats;

            await _trainService.Update(train.Id, train);
            await _reservationService.Remove(reservationId);

            ApiResponse<Reservation> apiResponse = new ApiResponse<Reservation>()
            {
                Success = true,
                Message = "Reservation deleted successfully",
                Data = reservation
            };

            return Ok(apiResponse);
        }
    }
}