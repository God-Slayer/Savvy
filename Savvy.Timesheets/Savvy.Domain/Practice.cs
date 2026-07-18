namespace Savvy.Domain;

/// <summary>Represents a healthcare practice that owns shifts and payment runs.</summary>
public sealed class Practice : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public ICollection<PaymentRun> PaymentRuns { get; set; } = new List<PaymentRun>();
}
