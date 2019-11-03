using LandonApi.Infrastructure;
using LandonApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Controllers
{
    [Route("/")]
    [ApiController]
    [ApiVersion("1.0")]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        [ProducesResponseType(200)]
        [ProducesResponseType(304)]
        [ResponseCache(CacheProfileName = "Static")]
        [Etag]
        public IActionResult GetRoot()
        {
            var response = new RootResponse
            {
                Self = Link.To(nameof(GetRoot)),
                Rooms = Link.ToCollection(nameof(RoomsController.GetAllRooms)),
                Info = Link.To(nameof(InfoController.GetInfo)),
                Users = Link.ToCollection(nameof(UsersController.GetVisibleUsers)),
                Token = FormMetadata.FromModel(
                    new PasswordGrantForm(),
                    Link.ToForm(nameof(TokenController.TokenExchange),
                                null, relations: Form.Relation))
            };

            if (!Request.GetEtagHandler().NoneMatch(response))
            {
                return StatusCode(304, response);
            }

            return Ok(response);
        }
    }
}
