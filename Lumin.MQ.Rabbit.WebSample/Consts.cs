namespace Lumin.MQ.Rabbit.WebSample
{
    public static class Consts
    {
        public static string RabbitHubOptions = nameof(RabbitHubOptions);
    }

    public static class MyHubs
    {
        public static string ShangHai = nameof(ShangHai);
    }

    public static class MyQueues
    {
        public static Queue hello = new Queue { Name = "hello" };
    }
}
