using System.ComponentModel.DataAnnotations;

namespace Vidly.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the customer's name.")]
    [StringLength(255)]
    public string Name { get; set; } = "";

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [Min18YearsIfAMember]
    public DateTime? Birthdate { get; set; }

    [Display(Name = "Subscribed to newsletter")]
    public bool IsSubscribedToNewsletter { get; set; }

    public MembershipType? MembershipType { get; set; }

    [Display(Name = "Membership Type")]
    [Required(ErrorMessage = "Please select a membership type.")]
    public byte? MembershipTypeId { get; set; }
}
