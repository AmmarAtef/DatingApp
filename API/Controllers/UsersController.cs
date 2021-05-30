using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository
            , IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var users = await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        ////api/users/3
        //[HttpGet("{id}")]
        //public async Task<ActionResult<AppUser>> GetUser(int id)
        //{
        //    var user = await _userRepository.GetUserByIdAsync(id);
        //    return Ok(user);
        //}

        //api/users/username
        [HttpGet("{username}", Name = "GetUserByName")]
        public async Task<ActionResult<MemberDto>> GetUserByName(string username)
        {
            var user = await _userRepository.GetUserByUserName(username);
            var userToReturn = _mapper.Map<MemberDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUserName();
            var user = await _userRepository.GetUserByUserName(username);
            _mapper.Map(memberUpdateDto, user);
            _userRepository.update(user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to Update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUserName();
            var user = await _userRepository.GetUserByUserName(username);
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _userRepository.SaveAllAsync())
            {
                //return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUserByName", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }


            return BadRequest("Problem Adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserName(User.GetUserName());
            var photo = user.Photos.SingleOrDefault(c => c.Id == photoId);
            if (photo.IsMain) return BadRequest("this is already your main Photo");
            var currentMain = user.Photos.FirstOrDefault(c => c.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed  to set Main Photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserName(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(c => c.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You can't delete your main photo");
            if(photo.PublicId != null)
            {
              var result=   await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("failed to Delete the Photo");
        }

    }
}
