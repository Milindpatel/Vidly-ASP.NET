using System.ComponentModel.DataAnnotations;

namespace Vidly.Models;

/// <summary>Customers on a paid membership must be at least 18. Pay-as-you-go customers have no age limit.</summary>
public class Min18YearsIfAMember : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var customer = (Customer)validationContext.ObjectInstance;

        if (customer.MembershipTypeId is null or MembershipType.Unknown or MembershipType.PayAsYouGo)
            return ValidationResult.Success;

        if (customer.Birthdate is not { } birthdate)
            return new ValidationResult("Birthdate is required for paid memberships.");

        return AgeOn(birthdate, DateTime.Today) >= 18
            ? ValidationResult.Success
            : new ValidationResult("Customer should be at least 18 years old to go on a membership.");
    }

    public static int AgeOn(DateTime birthdate, DateTime today)
    {
        var age = today.Year - birthdate.Year;
        if (birthdate.Date > today.Date.AddYears(-age))
            age--;
        return age;
    }
}
