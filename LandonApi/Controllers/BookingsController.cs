using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authzService;
        private readonly PagingOptions _defaultPagingOptions;

        public BookingsController(
            IBookingService bookingService,
            IUserService userService,
            IAuthorizationService authzService,
            IOptions<PagingOptions> defaultPagingOptionsAccessor)
        {
            _bookingService = bookingService;
            _userService = userService;
            _authzService = authzService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
        }

        [Authorize]
        [HttpGet(Name = nameof(GetVisibleBookings))]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public async Task<PagedCollection<Booking>> GetVisibleBookings(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Booking, BookingEntity> sortOptions,
            [FromQuery] SearchOptions<Booking, BookingEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var bookings = new PagedResults<Booking>();

            if (User.Identity.IsAuthenticated)
            {
                var userCanSeeAllBookings = await _authzService.AuthorizeAsync(
                    User, "ViewAllBookingsPolicy");
                if (userCanSeeAllBookings.Succeeded)
                {
                    bookings = await _bookingService.GetBookingsAsync(
                        pagingOptions, sortOptions, searchOptions);
                }
                else
                {
                    var userId = await _userService.GetUserIdAsync(User);
                    if (userId != null)
                    {
                        bookings = await _bookingService.GetBookingsForUserIdAsync(
                            userId.Value, pagingOptions, sortOptions, searchOptions);
                    }
                }
            }

            var collectionLink = Link.ToCollection(nameof(GetVisibleBookings));
            var collection = PagedCollection<Booking>.Create(
                collectionLink,
                bookings.Items.ToArray(),
                bookings.TotalSize,
                pagingOptions);

            return collection;
        }

        [Authorize]
        [HttpGet("{bookingId}", Name = nameof(GetBookingById))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Booking>> GetBookingById(Guid bookingId)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return NotFound();

            Booking booking = null;

            var canViewAllBookings = await _authzService.AuthorizeAsync(
                User, "ViewAllBookingsPolicy");

            if (canViewAllBookings.Succeeded)
            {
                booking = await _bookingService.GetBookingAsync(bookingId);
            }
            else
            {
                booking = await _bookingService.GetBookingForUserIdAsync(
                    bookingId, userId.Value);
            }

            if (booking == null) return NotFound();

            return booking;
        }

        // DELETE /bookings/{bookingId}
        [Authorize]
        [HttpDelete("{bookingId}", Name = nameof(DeleteBookingById))]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBookingById(Guid bookingId)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return NotFound();

            var booking = await _bookingService.GetBookingForUserIdAsync(
                bookingId, userId.Value);
            if (booking != null)
            {
                await _bookingService.DeleteBookingAsync(bookingId);
                return NoContent();
            }

            var canViewAllBookings = await _authzService.AuthorizeAsync(
                User, "ViewAllBookingsPolicy");
            if (!canViewAllBookings.Succeeded)
            {
                return NotFound();
            }

            booking = await _bookingService.GetBookingAsync(bookingId);
            if (booking == null) return NotFound();

            await _bookingService.DeleteBookingAsync(bookingId);
            return NoContent();
        }
    }

}
