﻿using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Entities;
using AutoMapper;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public MessagesController(IUnitOfWork unitOfWork
            , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You can't send message to yourSelf");
            }
            var sender = await _unitOfWork.UserRepository.GetUserByUserName(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserName(createMessageDto.RecipientUsername);
            if (recipient == null)
                return NotFound();


            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to send Message");
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUsers([FromQuery]
        MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUsers(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPages);

            return messages;
        }


        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUserName();
            return Ok(await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.Sender.UserName != username &&
                message.Recipient.UserName != username)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == username)
                message.SenderDeleted = true;
            if (message.Recipient.UserName == username)
                message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Problem deleting the message");
        }


    }
}
