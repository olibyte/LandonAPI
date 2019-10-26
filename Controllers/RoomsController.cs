using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService)
        {
            _roomService = roomService;
            _openingService = openingService;
        }

        // GET /rooms
        [HttpGet(Name = nameof(GetAllRooms))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Room>>> GetAllRooms()
        {
            var rooms = await _roomService.GetRoomsAsync();

            var collection = new Collection<Room>
            {
                Self = Link.ToCollection(nameof(GetAllRooms)),
                Value = rooms.ToArray()
            };

            return collection;
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpenings))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Opening>>> GetAllRoomOpenings(
            [FromQuery] PagingOptions pagingOptions = null)
        {
            var openings = await _openingService.GetOpeningsAsync(pagingOptions);

            var collection = new PagedCollection<Opening>()
            {
                Self = Link.ToCollection(nameof(GetAllRoomOpenings)),
                Value = openings.Items.ToArray(),
                Size = openings.TotalSize,
                Offset = pagingOptions.Offset.Value,
                Limit = pagingOptions.Limit.Value
            };

            return collection;
        }

        // GET /rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomById))]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Room>> GetRoomById(Guid roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            return room;
        }
    }
}
