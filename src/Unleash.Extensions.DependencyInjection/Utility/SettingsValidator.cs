using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Unleash.Utility
{
    public static class SettingsValidator
    {
        public static void Validate<TSettings>(TSettings settings)
            where TSettings : class
        {
            var context = new ValidationContext(settings, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(settings, context, results, validateAllProperties: true))
            {
                throw new AggregateException(results.Select(result => new ValidationException(result, null, settings)));
            }
        }
    }
}
