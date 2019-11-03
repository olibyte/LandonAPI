using LandonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public interface IBookingService
    {
        Task<Booking> GetBookingAsync(Guid bookingId);

        Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt);

        Task DeleteBookingAsync(Guid bookingId);

        Task<PagedResults<Booking>> GetBookingsAsync(
            PagingOptions pagingOptions,
            SortOptions<Booking, BookingEntity> sortOptions,
            SearchOptions<Booking, BookingEntity> searchOptions);

        Task<Booking> GetBookingForUserIdAsync(
            Guid bookingId,
            Guid userId);

        Task<PagedResults<Booking>> GetBookingsForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<Booking, BookingEntity> sortOptions,
            SearchOptions<Booking, BookingEntity> searchOptions);
    }

}
