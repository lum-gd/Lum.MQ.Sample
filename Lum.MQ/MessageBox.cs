namespace Lum.MQ
{
    public interface IMessageBox
    {
        string Name { get; }
    }

    public class Queue : IMessageBox
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return "Queue - " + Name;
        }
    }

    public class Topic : IMessageBox
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return "Topic - " + Name;
        }
    }
}