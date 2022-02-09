namespace Lum.MQ.Rabbit.WorkerSample
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
