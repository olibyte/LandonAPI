using AspNet.Security.OpenIdConnect.Primitives;
using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Controllers
{
    [Route("/[controller]")]
    [Authorize]
    [ApiController]
    public class UserinfoController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserinfoController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /userinfo
        [HttpGet(Name = nameof(Userinfo))]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserinfoResponse>> Userinfo()
        {
            var user = await _userService.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user does not exist."
                });
            }
            var userId = _userService.GetUserIdAsync(User);

            return new UserinfoResponse
            {
                Self = Link.To(nameof(Userinfo)),
                GivenName = user.FirstName,
                FamilyName = user.LastName,
                Subject = Url.Link(
                    nameof(UsersController.GetUserById),
                    new { userId })
            };
        }
    }
}
