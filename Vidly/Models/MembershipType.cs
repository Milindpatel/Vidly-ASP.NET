using System.ComponentModel.DataAnnotations;

namespace Vidly.Models;

public class MembershipType
{
    public const byte Unknown = 0;
    public const byte PayAsYouGo = 1;

    public byte Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = "";

    public short SignUpFee { get; set; }
    public byte DurationInMonths { get; set; }
    public byte DiscountRate { get; set; }
}
