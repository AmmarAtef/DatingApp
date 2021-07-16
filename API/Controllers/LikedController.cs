using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class LikedController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikedController(IUserRepository userRepository
            , ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByUserName(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
            if (likedUser == null)
            {
                return NotFound();
            }

            if (sourceUser.UserName == username)
            {
                return BadRequest("you Can't Like Yourself");
            }

            var userLike = await _likesRepository
                .GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null)
                return BadRequest("you already Like this User");

             userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id,
                SourceUser = sourceUser,
                LikedUser = likedUser
            };

            sourceUser.LikedUsers.Add(userLike);
            if (await _likesRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to like user ");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
        {
            int userId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(predicate, userId);
            return Ok(users);
        }




    }
}
