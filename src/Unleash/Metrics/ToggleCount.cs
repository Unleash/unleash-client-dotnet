namespace Unleash.Metrics
{
    internal class ToggleCount
    {
        public ToggleCount()
        {
            Clear();
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

        public void Clear()
        {
            Yes = 0;
            No = 0;
        }

        public long Yes { get; private set; }
        public long No { get; private set; }
    }
}