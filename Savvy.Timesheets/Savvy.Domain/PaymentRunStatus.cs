namespace Savvy.Domain;

/// <summary>Defines the lifecycle of a payment batch.</summary>
public enum PaymentRunStatus
{
    Draft,
    Processed,
    Cancelled,
}
