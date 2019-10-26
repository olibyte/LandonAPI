using LandonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public interface IOpeningService
    {
        Task<PagedResults<Opening>> GetOpeningsAsync(PagingOptions pagingOptions);

        Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end);
    }
}
