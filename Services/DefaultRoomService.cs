using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LandonAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LandonAPI.Services
{
    public class DefaultRoomService : IRoomService
    {
        private readonly HotelApiDbContext _context;
        private readonly IMapper _mapper;
        public DefaultRoomService(
            HotelApiDbContext context,
            IMapper mapper
            )
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<Room> GetRoomAsync(Guid id)
        {
            var entity = await _context.Rooms
                            .SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            return _mapper.Map<Room>(entity);
        }
    }
}
