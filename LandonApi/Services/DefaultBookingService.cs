using AutoMapper;
using AutoMapper.QueryableExtensions;
using LandonApi.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly IConfigurationProvider _mappingConfiguration;
        private readonly UserManager<UserEntity> _userManager;

        public DefaultBookingService(
            HotelApiDbContext context,
            IDateLogicService dateLogicService,
            IConfigurationProvider mappingConfiguration,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _dateLogicService = dateLogicService;
            _mappingConfiguration = mappingConfiguration;
            _userManager = userManager;
        }

        public async Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("You must be logged in.");

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
                User = user,
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
                .Include(b => b.User)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(b => b.Id == bookingId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Booking>(entity);
        }

        public async Task<Booking> GetBookingForUserIdAsync(Guid bookingId, Guid userId)
        {
            var entity = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(b => b.Id == bookingId && b.User.Id == userId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Booking>(entity);
        }

        public async Task<PagedResults<Booking>> GetBookingsAsync(
            PagingOptions pagingOptions,
            SortOptions<Booking, BookingEntity> sortOptions,
            SearchOptions<Booking, BookingEntity> searchOptions)
        {
            IQueryable<BookingEntity> query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync();

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Booking>(_mappingConfiguration)
                .ToArrayAsync();

            return new PagedResults<Booking>
            {
                Items = items,
                TotalSize = size
            };
        }

        public async Task<PagedResults<Booking>> GetBookingsForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<Booking, BookingEntity> sortOptions,
            SearchOptions<Booking, BookingEntity> searchOptions)
        {
            IQueryable<BookingEntity> query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Where(b => b.User.Id == userId);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync();

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Booking>(_mappingConfiguration)
                .ToArrayAsync();

            return new PagedResults<Booking>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
