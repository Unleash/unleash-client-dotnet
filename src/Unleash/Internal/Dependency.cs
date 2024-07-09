namespace Unleash.Internal
{
    public class Dependency
    {
        /// <summary>
        /// Feature is the name of the feature toggle we depend upon
        /// </summary>
        public string Feature { get; }
        /// <summary>
        /// Variants contains a string of variants that the dependency should resolve to
        /// </summary>
        public string[] Variants { get; }
        /// <summary>
        /// Enabled is the property that determines whether the dependency should be on or off. 
        /// If the property is absent from the payload it's assumed to be default on
        /// </summary>
        public bool Enabled { get; }

        public Dependency(string feature, string[] variants = null, bool? enabled = null)
        {
            Feature = feature;
            Variants = variants ?? new string[0];
            Enabled = enabled ?? true;
        }
    }
}