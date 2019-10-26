using AutoMapper;
using LandonApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public class DefaultBookingService : IBookingService
    {
        private readonly HotelApiDbContext _context;
        private readonly IDateLogicService _dateLogicService;
        private readonly IMapper _mapper;

        public DefaultBookingService(
            HotelApiDbContext context,
            IDateLogicService dateLogicService,
            IMapper mapper)
        {
            _context = context;
            _dateLogicService = dateLogicService;
            _mapper = mapper;
        }

        public Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            // TODO: Save the new booking to the database
            throw new NotImplementedException();
        }

        public async Task<Booking> GetBookingAsync(Guid bookingId)
        {
            var entity = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId);

            if (entity == null) return null;

            return _mapper.Map<Booking>(entity);
        }
    }
}
