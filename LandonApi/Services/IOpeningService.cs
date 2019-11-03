using LandonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public interface IOpeningService
    {
        Task<PagedResults<Opening>> GetOpeningsAsync(
            PagingOptions pagingOptions,
            SortOptions<Opening, OpeningEntity> sortOptions,
            SearchOptions<Opening, OpeningEntity> searchOptions);

        Task<PagedResults<Opening>> GetOpeningsByRoomIdAsync(
            Guid roomId,
            PagingOptions pagingOptions,
            SortOptions<Opening, OpeningEntity> sortOptions,
            SearchOptions<Opening, OpeningEntity> searchOptions);

        Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end);
    }
}
