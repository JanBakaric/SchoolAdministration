namespace AdministracijaSkole.Web.Models;

public class MessageViewModel
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string SenderUserName { get; set; }
    public string SenderRole { get; set; }
    public string ReceiverId { get; set; }
    public string ReceiverUserName { get; set; }
    public string ReceiverRole { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime SentAt { get; set; }
}
