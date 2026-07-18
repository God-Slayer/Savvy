namespace Savvy.Application.Common;

/// <summary>Centralizes assignment-mandated time and monetary rounding.</summary>
/// <summary>Centralizes assignment-specific hours and currency rounding calculations.</summary>
public static class CalculationHelper
{
    public static decimal HoursWorked(DateTime startUtc, DateTime endUtc, int? unpaidBreakMinutes)
    {
        var hours = (decimal)(endUtc - startUtc).TotalHours - (unpaidBreakMinutes ?? 0) / 60m;
        return Math.Round(Math.Max(0m, hours), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Rounds a monetary value to two decimal places away from zero.</summary>
    public static decimal Money(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
