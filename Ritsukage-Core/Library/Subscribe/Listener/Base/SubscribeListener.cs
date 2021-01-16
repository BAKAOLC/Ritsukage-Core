namespace Ritsukage.Library.Subscribe.Listener.Base
{
    public abstract class SubscribeListener
    {
        public abstract void RefreshListener();

        public abstract void Listen();

        public abstract void Broadcast(CheckResult.Base.SubscribeCheckResult result);
    }
}
