using AutoMapper;
using AutoMapper.QueryableExtensions;
using LandonApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public class DefaultOpeningService : IOpeningService
    {
        private readonly HotelApiDbContext _context;
        private readonly IDateLogicService _dateLogicService;
        private readonly IConfigurationProvider _mappingConfiguration;

        public DefaultOpeningService(
            HotelApiDbContext context,
            IDateLogicService dateLogicService,
            IConfigurationProvider mappingConfiguration)
        {
            _context = context;
            _dateLogicService = dateLogicService;
            _mappingConfiguration = mappingConfiguration;
        }

        public async Task<PagedResults<Opening>> GetOpeningsAsync(
            PagingOptions pagingOptions,
            SortOptions<Opening, OpeningEntity> sortOptions,
            SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            var rooms = await _context.Rooms.ToArrayAsync();

            return await GetOpeningsForRoomsAsync(
                rooms, pagingOptions, sortOptions, searchOptions);
        }

        public async Task<PagedResults<Opening>> GetOpeningsByRoomIdAsync(
            Guid roomId,
            PagingOptions pagingOptions,
            SortOptions<Opening, OpeningEntity> sortOptions,
            SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            var room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);
            if (room == null) throw new ArgumentException("Invalid room id.");

            return await GetOpeningsForRoomsAsync(
                new[] { room }, pagingOptions, sortOptions, searchOptions);
        }

        private async Task<PagedResults<Opening>> GetOpeningsForRoomsAsync(
            RoomEntity[] rooms,
            PagingOptions pagingOptions,
            SortOptions<Opening, OpeningEntity> sortOptions,
            SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            var allOpenings = new List<OpeningEntity>();

            foreach (var room in rooms)
            {
                // Generate a sequence of raw opening slots
                var allPossibleOpenings = _dateLogicService.GetAllSlots(
                        DateTimeOffset.UtcNow,
                        _dateLogicService.FurthestPossibleBooking(DateTimeOffset.UtcNow))
                    .ToArray();

                var conflictedSlots = await GetConflictingSlots(
                    room.Id,
                    allPossibleOpenings.First().StartAt,
                    allPossibleOpenings.Last().EndAt);

                // Remove the slots that have conflicts and project
                var openings = allPossibleOpenings
                    .Except(conflictedSlots, new BookingRangeComparer())
                    .Select(slot => new OpeningEntity
                    {
                        RoomId = room.Id,
                        Rate = room.Rate,
                        StartAt = slot.StartAt,
                        EndAt = slot.EndAt
                    });

                allOpenings.AddRange(openings);
            }

            var pseudoQuery = allOpenings.AsQueryable();
            pseudoQuery = searchOptions.Apply(pseudoQuery);
            pseudoQuery = sortOptions.Apply(pseudoQuery);

            var size = pseudoQuery.Count();

            var items = pseudoQuery
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Opening>(_mappingConfiguration)
                .ToArray();

            return new PagedResults<Opening>
            {
                Items = items,
                TotalSize = size
            };
        }

        public async Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end)
        {
            return await _context.Bookings
                .Where(b => b.Room.Id == roomId && _dateLogicService.DoesConflict(b, start, end))
                // Split each existing booking up into a set of atomic slots
                .SelectMany(existing => _dateLogicService
                    .GetAllSlots(existing.StartAt, existing.EndAt))
                .ToArrayAsync();
        }
    }
}
