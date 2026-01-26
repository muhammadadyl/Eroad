namespace Eroad.RouteManagement.Common
{
    public class Checkpoint
    {
        public int Sequence { get; set; }
        public string Location { get; set; }
        public DateTime ExpectedTime { get; set; }

        public Checkpoint() { }

        public Checkpoint(int sequence, string location, DateTime expectedTime)
        {
            Sequence = sequence;
            Location = location;
            ExpectedTime = expectedTime;
        }
    }
}
