using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace REST_API_NET8.Models.SAP
{
    /// <summary>
    /// Agument Filter Data
    /// </summary>
    public class getDataSapFilter : IValidatableObject
    {
        /// <summary>
        /// 9771 9772 9773 9774
        /// </summary>
        [Required(ErrorMessage = "Error Facuty Require!")]
        public string? plant { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var allowPlant = new HashSet<string> { "9771", "9772", "9773", "9774" };
            if (plant == null || !allowPlant.Contains(plant))
            {
                yield return new ValidationResult(
                     $"Plant Code must be one of the following: {string.Join(", ", allowPlant)}.",
                     new[] { nameof(plant) }
                 );
            }
        }

        /// <summary>
        /// Format Condition Example is B%
        /// </summary>
        public string? condition { get; set; }
        /// <summary>
        /// Format yyyyMMdd Example is 20260527
        /// </summary>
        public string? startDate { get; set; }
        /// <summary>
        /// Format yyyyMMdd Example is 20260527
        /// </summary>
        public string? endDate { get; set; }
    }
}