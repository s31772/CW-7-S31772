using System.ComponentModel.DataAnnotations;

namespace apbd_cw7.Models.DTOs;

public class ClientCreateDTO
{
    [Length(1,120)]
    public required string FirstName { get; set; }
    [Length(1,120)]
    public required string LastName { get; set; }
    [Length(1,120)]
    [RegularExpression(@".+@.+\..+", ErrorMessage = "Email musi zawierać znak '@' oraz kropkę po '@'.")]
    public required string Email { get; set; }
    [Length(1,120)]
    [RegularExpression(@"^\+\d+$", ErrorMessage = "Telefon musi zaczynać się od '+' i zawierać tylko cyfry.")]
    public required string Telephone { get; set; }
    [Length(1,120)]
    public required string Pesel { get; set; }
}