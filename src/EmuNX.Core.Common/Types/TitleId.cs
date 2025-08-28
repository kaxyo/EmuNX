namespace EmuNX.Core.Common.Types;

/// <summary>
/// Stores a ROM title identifier like 01006B601380E000.
/// Provides utility methods for both representation and instantiation.
/// </summary>
public readonly struct TitleId : IEquatable<TitleId>, IComparable<TitleId>
{
    public readonly ulong Num;

    public TitleId(ulong num)
    {
        Num = num;
    }

    public static bool TryParseHex(string hex, out TitleId titleId)
    {
        // Validation
        var isLengthBad = string.IsNullOrEmpty(hex) || hex.Length != 16;
        var failedToParse = !ulong.TryParse(hex.ToUpper(), System.Globalization.NumberStyles.HexNumber, null, out var number);

        // Instantiation
        var success = !(isLengthBad || failedToParse);
        titleId = new TitleId(success ? number : 0);
        return success;
    }

    public string Hex => Num.ToString("X16");

    public override string ToString() => Hex;

    public bool Equals(TitleId other) => Num == other.Num;

    public override int GetHashCode() => Num.GetHashCode();

    public override bool Equals(object? obj) => obj is TitleId other && Equals(other);

    public static bool operator ==(TitleId left, TitleId right) => left.Equals(right);

    public static bool operator !=(TitleId left, TitleId right) => !(left == right);

    public int CompareTo(TitleId other) => Num.CompareTo(other.Num);
}