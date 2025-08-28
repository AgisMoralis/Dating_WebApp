using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseAPIController
{
    [HttpPost]
    public async Task<ActionResult<Models.MessageDto>> CreateMessageAsync(Models.CreateMessageDto newMessage)
    {
        var username = User.GetUsername();
        if (username == newMessage.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot message yourself");
        }

        var sender = await unitOfWork.UserRepository.GetMemberByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetMemberByUsernameAsync(newMessage.RecipientUsername);
        if (sender is null || recipient is null || sender.UserName == null || recipient.UserName == null)
        {
            return BadRequest("Cannot send message at this time");
        }

        var message = new Entities.Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = newMessage.Content
        };
        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.CompleteAsync())
        {
            return Ok(mapper.Map<Models.MessageDto>(message));
        }
        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MessageDto>>> GetMessagesForUserAsync([FromQuery] Models.MessageParametersDto messageParams)
    {
        messageParams.CurrentUsername = User.GetUsername();
        var messages = await unitOfWork.MessageRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<Models.MessageDto>>> GetThreadMessagesAsync(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await unitOfWork.MessageRepository.GetThreadMessagesAsync(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessagesAsync(int id)
    {
        var username = User.GetUsername();

        var message = await unitOfWork.MessageRepository.GetMessageAsync(id);
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
            unitOfWork.MessageRepository.DeleteMessage(message);
        }

        if (await unitOfWork.CompleteAsync())
        {
            return Ok();
        }
        return BadRequest("Problem deleting the message");
    }
}
