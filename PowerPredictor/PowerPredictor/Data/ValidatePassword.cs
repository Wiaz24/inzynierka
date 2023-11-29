using System.ComponentModel.DataAnnotations;

namespace PowerPredictor.Data
{ 
    /// <summary>
    /// Password validation data annotation
    /// </summary>
    public class ValidatePassword : ValidationAttribute
    {
        private readonly int minLenght = 6;
        private readonly bool requireNumber = true;
        private readonly bool requireUppercase = true;
        private readonly bool requireNonAlphanumeric = false;

        public ValidationResult? IsValidForTests(object? value,
                    ValidationContext validationContext)
        {
            return IsValid(value, validationContext);
        }
        
        /// <summary>
        /// Checks if password is valid
        /// </summary>
        /// <param name="value"> Provided password </param>
        /// <param name="validationContext"> Form context </param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value,
                    ValidationContext validationContext)
        {
            string password = (value as string) ??  string.Empty;
            
            if (password == null)
                password = string.Empty;

            if (password.Length < minLenght)
                return new ValidationResult($"Password must be at least {minLenght} characters long",
                                           new[] { validationContext.MemberName });

            bool hasNumber = false;
            bool hasNonAlphanumeric = false;
            bool hasUppercase = false;

            foreach (char c in password)
            {
                if (char.IsDigit(c))
                    hasNumber = true;

                if (char.IsUpper(c))
                    hasUppercase = true;

                if (!char.IsLetterOrDigit(c))
                    hasNonAlphanumeric = true;
            }

            if (requireNumber && !hasNumber)
                return new ValidationResult("Password must contain at least one number",
                                                          new[] { validationContext.MemberName });

            if (requireUppercase && !hasUppercase)
                return new ValidationResult("Password must contain at least one uppercase letter",
                                                                             new[] { validationContext.MemberName });

            if (requireNonAlphanumeric && !hasNonAlphanumeric)
                return new ValidationResult("Password must contain at least one non alphanumeric character",
                                                                                                        new[] { validationContext.MemberName });
            return null;
        }
    }
}
