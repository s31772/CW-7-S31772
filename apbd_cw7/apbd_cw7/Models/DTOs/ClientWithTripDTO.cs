namespace apbd_cw7.Models.DTOs;

public class ClientWithTripDTO
{
    public TripGetDTO Trip { get; set; }
    public int? RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}