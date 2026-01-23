namespace Eroad.RouteManagement.Common
{
    public class Checkpoint
    {
        public int Sequence { get; set; }
        public string Location { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime? ActualTime { get; set; }

        public Checkpoint() { }

        public Checkpoint(int sequence, string location, DateTime expectedTime, DateTime? actualTime = null)
        {
            Sequence = sequence;
            Location = location;
            ExpectedTime = expectedTime;
            ActualTime = actualTime;
        }
    }
}
