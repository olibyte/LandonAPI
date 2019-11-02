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

        public async Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            var room = await _context.Rooms
                .SingleOrDefaultAsync(r => r.Id == roomId);
            if (room == null) throw new ArgumentException("Invalid room ID.");

            var minimumStay = _dateLogicService.GetMinimumStay();
            var total = (int)((endAt - startAt).TotalHours / minimumStay.TotalHours)
                        * room.Rate;

            var id = Guid.NewGuid();

            var newBooking = _context.Bookings.Add(new BookingEntity
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                StartAt = startAt.ToUniversalTime(),
                EndAt = endAt.ToUniversalTime(),
                Total = total,
                Room = room
            });

            var created = await _context.SaveChangesAsync();
            if (created < 1) throw new InvalidOperationException("Could not create booking.");

            return id;
        }

        public async Task DeleteBookingAsync(Guid bookingId)
        {
            var booking = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
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
