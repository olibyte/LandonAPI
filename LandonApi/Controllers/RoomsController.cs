using LandonApi.Infrastructure;
using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly IDateLogicService _dateLogicService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly PagingOptions _defaultPagingOptions;

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService,
            IDateLogicService dateLogicService,
            IBookingService bookingService,
            IUserService userService,
            IOptions<PagingOptions> defaultPagingOptionsWrapper)
        {
            _roomService = roomService;
            _openingService = openingService;
            _dateLogicService = dateLogicService;
            _bookingService = bookingService;
            _userService = userService;
            _defaultPagingOptions = defaultPagingOptionsWrapper.Value;
        }

        // GET /rooms
        [HttpGet(Name = nameof(GetAllRooms))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Room>>> GetAllRooms(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions,
            [FromQuery] SearchOptions<Room, RoomEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetRoomsAsync(
                pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Room>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetAllRooms)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);
            collection.Openings = Link.ToCollection(nameof(GetAllRoomOpenings));
            collection.RoomsQuery = FormMetadata.FromResource<Room>(
                Link.ToForm(
                    nameof(GetAllRooms),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return collection;
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpenings))]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        [ResponseCache(Duration = 30,
            VaryByQueryKeys = new[] { "offset", "limit", "orderBy", "search" })]
        public async Task<ActionResult<Collection<Opening>>> GetAllRoomOpenings(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Opening, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var openings = await _openingService.GetOpeningsAsync(
                pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Opening>.Create<OpeningsResponse>(
                Link.ToCollection(nameof(GetAllRoomOpenings)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<Opening>(
                Link.ToForm(
                    nameof(GetAllRoomOpenings),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return collection;
        }

        // GET /rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomById))]
        [ProducesResponseType(304)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ResponseCache(CacheProfileName = "Resource")]
        [Etag]
        public async Task<ActionResult<Room>> GetRoomById(Guid roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            if (!Request.GetEtagHandler().NoneMatch(room))
            {
                return StatusCode(304, room);
            }

            return room;
        }

        // POST /rooms/{roomId}/bookings
        [Authorize]
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoom))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(201)]
        public async Task<ActionResult> CreateBookingForRoom(
            Guid roomId, [FromBody] BookingForm form)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();

            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            var minimumStay = _dateLogicService.GetMinimumStay();
            bool tooShort = (form.EndAt.Value - form.StartAt.Value) < minimumStay;
            if (tooShort) return BadRequest(new ApiError(
                $"The minimum booking duration is {minimumStay.TotalHours} hours."));

            var conflictedSlots = await _openingService.GetConflictingSlots(
                roomId, form.StartAt.Value, form.EndAt.Value);
            if (conflictedSlots.Any()) return BadRequest(new ApiError(
                "This time conflicts with an existing booking."));

            var bookingId = await _bookingService.CreateBookingAsync(
                userId.Value, roomId, form.StartAt.Value, form.EndAt.Value);

            return Created(
                Url.Link(nameof(BookingsController.GetBookingById),
                new { bookingId }),
                null);
        }

        [HttpGet("{roomId}/openings", Name = nameof(GetRoomOpeningsByRoomId))]
        [ResponseCache(CacheProfileName = "Collection",
               VaryByQueryKeys = new[] { "roomId", "offset", "limit", "orderBy", "search" })]
        public async Task<IActionResult> GetRoomOpeningsByRoomId(
            Guid roomId,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Opening, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            var openings = await _openingService.GetOpeningsByRoomIdAsync(
                roomId,
                pagingOptions,
                sortOptions,
                searchOptions);

            var collectionLink = Link.ToCollection(
                nameof(GetRoomOpeningsByRoomId), new { roomId });

            var collection = PagedCollection<Opening>.Create<OpeningsResponse>(
                collectionLink,
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<Opening>(
                Link.ToForm(nameof(GetRoomOpeningsByRoomId),
                            new { roomId }, Link.GetMethod, Form.QueryRelation));

            return Ok(collection);
        }
    }
}
