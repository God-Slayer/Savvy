namespace Savvy.Domain;

public sealed class User : BaseEntity
{
    public Guid? PracticeId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public Practice? Practice { get; set; }

    public ICollection<Shift> AssignedShifts { get; set; } = new List<Shift>();

    public ICollection<PaymentRunLine> PaymentRunLines { get; set; } = new List<PaymentRunLine>();
}
