using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PowerPredictor.Data
{
    public class ValidateScrapperDate : ValidationAttribute
    {
        private readonly string otherDate;

        public ValidateScrapperDate(string otherDate)
        {
            this.otherDate = otherDate;
        }
        protected override ValidationResult? IsValid(object value,
                    ValidationContext validationContext)
        {
            DateOnly date = (DateOnly)value;

            var propertyInfo = validationContext.ObjectType.GetProperty(otherDate);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {otherDate}");
            }

            var otherValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (otherValue != null)
            {
                if (otherDate == "StartDate" && date < (DateOnly)otherValue
                    || otherDate == "EndDate" && date > (DateOnly)otherValue)
                    return new ValidationResult($"End date cannot be before start date");
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
