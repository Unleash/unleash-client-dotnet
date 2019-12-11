namespace Unleash.Communication.Admin.Dto
{
    public class ApplicationDetail : Application
    {
        public Instance[] Instances { get; set; }
        public string[] SeenToggles { get; set; }
    }
}
