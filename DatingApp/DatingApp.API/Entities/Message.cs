namespace DatingApp.API.Entities;

public class Message
{
    public int Id { get; set; }
    public required string SenderUsername { get; set; }
    public required string RecipientUsername { get; set; }
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime MessageSent { get; set; } = DateTime.Now;
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }

    // Navigation properties
    public int SenderId { get; set; }
    public Member Sender { get; set; } = null!;
    public int RecipientId { get; set; }
    public Member Recipient { get; set; } = null!;

}
