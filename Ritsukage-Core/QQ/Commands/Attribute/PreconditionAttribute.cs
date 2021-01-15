using Sora.EventArgs.SoraEvent;
using System;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class PreconditionAttribute : Attribute
    {
        public abstract Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args);
    }
}
