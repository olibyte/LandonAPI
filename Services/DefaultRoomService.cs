using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LandonAPI.Services
{
    public class DefaultRoomService : IRoomService
    {
        private readonly HotelApiDbContext _context;
        public DefaultRoomService(HotelApiDbContext context)
        {
            _context = context;
        }
        public async Task<Room> GetRoomAsync(Guid id)
        {
            var entity = await _context.Rooms
                            .SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            return new Room
            {
                Href = null, //Url.Link(nameof(GetRoomById), new { roomId = entity.Id }),
                Name = entity.Name,
                Rate = entity.Rate / 100.0m
            };
        }
    }
}
