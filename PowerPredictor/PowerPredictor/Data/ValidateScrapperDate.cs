using System.ComponentModel.DataAnnotations;

namespace PowerPredictor.Data
{
    public class ValidateScrapperDate : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value,
                    ValidationContext validationContext)
        {
            DateOnly date = (DateOnly)value;

            if (date >= DateOnly.FromDateTime(DateTime.Today))
                return new ValidationResult($"Latest possible date is yesterday",
                                       new[] { validationContext.MemberName });

            if (date < new DateOnly(day: 1, month: 1, year: 2000))
                return new ValidationResult($"Earliest possible date is 1.01.2000",
                                       new[] { validationContext.MemberName });

            return null;
        }
    }
}
