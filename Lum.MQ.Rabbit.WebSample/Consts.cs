namespace Lum.MQ.Rabbit.WebSample
{
    public static class MyHubs
    {
        public static string ShangHai = nameof(ShangHai);
    }

    public static class MyQueues
    {
        public static Queue hello = new Queue { Name = "hello" };
    }
}
