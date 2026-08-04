using System.ComponentModel.DataAnnotations;


namespace MotorsShop.Validation;

public class NotInFutureAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is int year && year > DateTime.UtcNow.Year)
            return new ValidationResult($"{context.MemberName} cannot be in the future.");
        return ValidationResult.Success;
    }
}