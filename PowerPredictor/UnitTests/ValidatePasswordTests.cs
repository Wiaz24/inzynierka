using System.ComponentModel.DataAnnotations;
using PowerPredictor.Data;

namespace UnitTests;

public class ValidatePasswordTests
{
    private readonly ValidatePassword validator;
    public ValidatePasswordTests()
    {
        validator = new ValidatePassword();
    }

    [Theory]
    [InlineData("Haslo123@#")]
    [InlineData("#addAFAG123")]
    [InlineData("UgaBuga123@")]
    public void IsValid_good_passwords(string password)
    {
        var validationContext = new ValidationContext(new object()); // Validation context doesn't need to be fully initialized

        ValidationResult? result = validator.IsValidForTests(password, validationContext);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_too_short_password()
    {
        var validationContext = new ValidationContext(new object()); // Validation context doesn't need to be fully initialized

        ValidationResult? result = validator.IsValidForTests("123", validationContext);
        Assert.NotNull(result);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Password must be at least 6 characters long", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_no_number()
    {
        var validationContext = new ValidationContext(new object()); // Validation context doesn't need to be fully initialized

        ValidationResult? result = validator.IsValidForTests("Haslo@#@", validationContext);
        Assert.NotNull(result);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Password must contain at least one number", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_no_uppercase()
    {
        var validationContext = new ValidationContext(new object()); // Validation context doesn't need to be fully initialized

        ValidationResult? result = validator.IsValidForTests("haslo123@#@", validationContext);
        Assert.NotNull(result);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Password must contain at least one uppercase letter", result.ErrorMessage);
    }
}
