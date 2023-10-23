using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PowerPredictor.Data
{
    /// <summary>
    /// Validates if date is in correct range
    /// </summary>
    public class ValidateScrapperDate : ValidationAttribute
    {
        /// <summary>
        /// Other DateOnly property name, which is used to compare dates
        /// </summary>
        private readonly string otherDate;

        public ValidateScrapperDate(string otherDate)
        {
            this.otherDate = otherDate;
        }

        /// <summary>
        /// Checks if date is in correct range
        /// </summary>
        /// <param name="value"> DateOnly date</param>
        /// <param name="validationContext"> Context for validation</param>
        /// <returns> ValidationResult if error or null if validated correctly </returns>
        /// <exception cref="NullReferenceException"> Exception thrown when incorrect other date is provided</exception>
        protected override ValidationResult? IsValid(object value,
                    ValidationContext validationContext)
        {
            DateOnly date = (DateOnly)value;

            var propertyInfo = validationContext.ObjectType.GetProperty(otherDate);
            if (propertyInfo == null)
            {
                throw new NullReferenceException("You have to provide a valid ther DateTime property name");
            }

            var otherValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (otherValue != null)
            {
                if (otherDate == "StartDate" && date < (DateOnly)otherValue
                    || otherDate == "EndDate" && date > (DateOnly)otherValue)
                    return new ValidationResult(
                        errorMessage: $"End date cannot be before start date",
                        memberNames: new[] { validationContext.MemberName });
                                        ;
            }

            if (date > DateOnly.FromDateTime(DateTime.Today))
                return new ValidationResult($"Latest possible date is today",
                                       new[] { validationContext.MemberName });

            if (date < new DateOnly(day: 1, month: 1, year: 2000))
                return new ValidationResult($"Earliest possible date is 1.01.2000",
                                       new[] { validationContext.MemberName });

            return null;
        }
    }
}
