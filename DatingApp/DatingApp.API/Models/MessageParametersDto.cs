namespace DatingApp.API.Models;

public class MessageParametersDto : PaginationParametersDto
{
    public string? CurrentUsername { get; set; }
    public string Container { get; set; } = "Unread";
}
