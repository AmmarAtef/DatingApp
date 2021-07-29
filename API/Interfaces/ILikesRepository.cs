using API.DTOs;
using API.Entities;
using API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int LikedUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        Task<PageList<LikeDto>> GetUserLikes(LikesParams likesParams);



    }
}
