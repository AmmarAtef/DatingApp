using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int LikedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, LikedUserId);
        }

        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (predicate == "liked")
            {
                likes = likes.Where(l => l.SourceUserId == userId);
                users = likes.Select(l => l.LikedUser);
            }
            if (predicate == "likedBy")
            {
                likes = likes.Where(l => l.LikedUserId == userId);
                users = likes.Select(l => l.SourceUser);
            }
            return await users.Select(user => new LikeDto
            {
                Username = user.UserName,
                Age = user.GetAge(),
                City = user.City,
                Id = user.Id,
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url
            }).ToListAsync();
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(c => c.Id == userId);
        }
    }
}
