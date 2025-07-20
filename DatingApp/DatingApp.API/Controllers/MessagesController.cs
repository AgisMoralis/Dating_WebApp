using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessagesAsync(int id)
    {
        var username = User.GetUsername();

        var message = await messageRepository.GetMessageAsync(id);
        if (message == null)
        {
            return BadRequest("Cannot delete this message");
        }

        if (message.SenderUsername != username && message.RecipientUsername != username)
        { 
            return Forbid();
        }
        if (message.SenderUsername == username)
        {
            message.SenderDeleted = true;
        }
        if (message.RecipientUsername == username)
        { 
            message.RecipientDeleted = true;
        }

        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            messageRepository.DeleteMessage(message);
        }

        if (await messageRepository.SaveAllAsync())
        {
            return Ok();
        }
        return BadRequest("Problem deleting the message");
    }
}
