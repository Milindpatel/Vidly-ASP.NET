using System.ComponentModel.DataAnnotations;
using Vidly.Models;

namespace Vidly.Tests;

public class ValidationTests
{
    private static bool IsValid(Customer customer)
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(customer, new ValidationContext(customer), results, validateAllProperties: true);
    }

    [Fact]
    public void PayAsYouGo_DoesNotNeedBirthdate()
    {
        Assert.True(IsValid(new Customer { Name = "Kid", MembershipTypeId = MembershipType.PayAsYouGo }));
    }

    [Fact]
    public void PaidMembership_RequiresBirthdate()
    {
        Assert.False(IsValid(new Customer { Name = "No DOB", MembershipTypeId = 2 }));
    }

    [Fact]
    public void PaidMembership_RequiresAdult()
    {
        var almost18 = DateTime.Today.AddYears(-18).AddDays(1);
        Assert.False(IsValid(new Customer { Name = "Teen", MembershipTypeId = 2, Birthdate = almost18 }));
        Assert.True(IsValid(new Customer { Name = "Adult", MembershipTypeId = 2, Birthdate = DateTime.Today.AddYears(-18) }));
    }

    [Theory]
    [InlineData("2008-06-15", "2026-06-14", 17)]
    [InlineData("2008-06-15", "2026-06-15", 18)]
    public void AgeOn_CountsBirthdaysCorrectly(string dob, string today, int expected)
    {
        Assert.Equal(expected, Min18YearsIfAMember.AgeOn(DateTime.Parse(dob), DateTime.Parse(today)));
    }
}
