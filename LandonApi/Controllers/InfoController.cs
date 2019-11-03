using LandonApi.Infrastructure;
using LandonApi.Models;
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
    public class InfoController : ControllerBase
    {
        private readonly HotelInfo _hotelInfo;

        public InfoController(IOptions<HotelInfo> hotelInfoWrapper)
        {
            _hotelInfo = hotelInfoWrapper.Value;
            _hotelInfo.Self = Link.To(nameof(GetInfo));
        }

        [HttpGet(Name = nameof(GetInfo))]
        [ProducesResponseType(200)]
        [ProducesResponseType(304)]
        [ResponseCache(CacheProfileName = "Static")]
        [Etag]
        public ActionResult<HotelInfo> GetInfo()
        {
            if (!Request.GetEtagHandler().NoneMatch(_hotelInfo))
            {
                return StatusCode(304, _hotelInfo);
            }

            return _hotelInfo;
        }
    }
}
