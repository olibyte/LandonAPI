using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApi.Models
{
    public class UserRoleEntity : IdentityRole<Guid>
    {
        public UserRoleEntity()
            : base()
        {
        }

        public UserRoleEntity(string roleName)
            : base(roleName)
        {
        }
    }
}
