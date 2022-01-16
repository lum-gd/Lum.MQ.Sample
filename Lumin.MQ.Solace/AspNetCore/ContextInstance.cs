using SolaceSystems.Solclient.Messaging;

namespace Lumin.MQ.Solace.AspNetCore
{
    public static class ContextInstance
    {
        static ContextInstance()
        {
            ContextFactoryProperties cfp = new ContextFactoryProperties
            {
                SolClientLogLevel = SolLogLevel.Warning
            };
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);
            SolaceContextInstance = ContextFactory.Instance.CreateContext(new ContextProperties
            {

            }, null);
        }
        public static IContext SolaceContextInstance { get; }
    }
}