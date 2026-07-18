namespace Savvy.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
