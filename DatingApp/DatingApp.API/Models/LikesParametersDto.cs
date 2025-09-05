namespace DatingApp.API.Models;

public class LikesParametersDto : PaginationParametersDto
{
    public int UserId { get; set; }
    public required string Predicate { get; set; } = "likes";
}
