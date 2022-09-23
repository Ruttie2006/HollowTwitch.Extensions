using HollowTwitch.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Precondition
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AdminOnlyAttribute : PreconditionAttribute
    {
        public override bool Check(string user)
        {
            return TwitchMod.Instance.Config.AdminUsers.Contains(user);
        }
    }
}
