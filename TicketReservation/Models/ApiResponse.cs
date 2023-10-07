namespace TicketReservation.Models;

public class ApiResponse<TResponseData>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    public TResponseData Data { get; set; }
}