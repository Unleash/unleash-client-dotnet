using System.ComponentModel.DataAnnotations;

namespace Unleash.Caching
{
    public abstract class BaseToggleCollectionCacheSettings
    {
        [Required]
        public string EtagKeyName { get; set; } = "Etags";

        [Required]
        public string ToggleCollectionKeyName { get; set; } = "Toggles";
    }
}
