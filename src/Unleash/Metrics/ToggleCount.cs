namespace Unleash.Metrics
{
    internal class ToggleCount
    {
        public ToggleCount()
        {
            Yes = 0;
            No = 0;
        }

        public void Register(bool active)
        {
            if (active)
            {
                Yes++;
            }
            else
            {
                No++;
            }
        }

        public long Yes { get; private set; }
        public long No { get; private set; }
    }
}