namespace Lum.MQ.Solace.AspNetCore
{
    public interface IHubIniter
    {
        string HubName { get; }
        void SubQueue(IMqHub mqHub);
        void SubTopic(IMqHub mqHub);
    }
}