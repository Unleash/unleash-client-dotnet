using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unleash;
using Unleash.Internal;
using Unleash.Variants;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnleashController : ControllerBase
    {
        private readonly ILogger<UnleashController> _logger;
        private readonly IUnleash _unleash;

        public UnleashController(ILogger<UnleashController> logger, IUnleash unleash)
        {
            _logger = logger;
            _unleash = unleash;
        }

        [HttpGet("status")]
        public string GetStatus()
        {
            _logger.LogInformation("Checking status");
            return "ok";
        }

        [HttpGet("variants/{toggleName}")]
        public IEnumerable<VariantDefinition> GetVariants([FromRoute] string toggleName)
        {
            _logger.LogInformation("Getting variants from toggle {0}", toggleName);
            return _unleash.GetVariants(toggleName);
        }

        [HttpGet("variants/{toggleName}/weighted")]
        public Variant GetVariant([FromRoute] string toggleName)
        {
            _logger.LogInformation("Getting weighted variant from toggle {0}", toggleName);
            return _unleash.GetVariant(toggleName);
        }

        [HttpGet("variants/{toggleName}/{variantName}")]
        public IEnumerable<VariantDefinition> GetVariantsByName([FromRoute] string toggleName, [FromRoute] string variantName)
        {
            _logger.LogInformation("Getting variants from toggle {0} and variant {1}", toggleName, variantName);
            return _unleash.GetVariants(toggleName).Where(v => v.Name == variantName);
        }
        
        [HttpGet("toggles/{toggleName}")]
        public bool GetToggle([FromRoute] string toggleName)
        {
            _logger.LogInformation("Getting toggle {0}", toggleName);
           return _unleash.IsEnabled(toggleName);
        }
    }
}
