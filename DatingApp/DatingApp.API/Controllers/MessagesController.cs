using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

public class MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper) : BaseAPIController
{
    [HttpPost]
    public async Task<ActionResult<Models.MessageDto>> CreateMessageAsync(Models.CreateMessageDto newMessage)
    {
        var username = User.GetUsername();
        if (username == newMessage.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot message yourself");
        }

        var sender = await userRepository.GetMemberByUsernameAsync(username);
        var recipient = await userRepository.GetMemberByUsernameAsync(newMessage.RecipientUsername);
        if (sender is null || recipient is null)
        {
            return BadRequest("Cannot send message at this time");
        }

        var message = new Entities.Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.Username,
            RecipientUsername = recipient.Username,
            Content = newMessage.Content
        };
        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync())
        {
            return Ok(mapper.Map<Models.MessageDto>(message));
        }
        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MessageDto>>> GetMessagesForUserAsync([FromQuery] Models.MessageParametersDto messageParams)
    {
        messageParams.CurrentUsername = User.GetUsername();
        var messages = await messageRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<Models.MessageDto>>> GetThreadMessagesAsync(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await messageRepository.GetThreadMessagesAsync(currentUsername, username));
    }
}
