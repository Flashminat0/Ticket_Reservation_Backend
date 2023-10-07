namespace TicketReservation.Models;

public class ApiFailedResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
}