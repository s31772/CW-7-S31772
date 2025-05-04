namespace apbd_cw7.Models.DTOs;

public class TripGetDTO
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DateFrom { get; set; }
    public string DateTo { get; set; }
    public int MaxPeople { get; set; }
}