namespace EmuNX.Core.Common.Types;

/// <summary>
/// Stores a ROM title identifier. Provides utility  methods. 
/// </summary>
public class TitleId
{
    public readonly ulong Num;

    public TitleId(ulong num)
    {
        Num = num;
    }

    public TitleId(string hex)
    {
        var isLengthBad = string.IsNullOrEmpty(hex) || hex.Length != 16;
        var wasNotParsed = !ulong.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out Num);
        
        if (isLengthBad || wasNotParsed)
            throw new ArgumentException("Invalid hexadecimal format for TitleId.");
    }

    public string Hex => Num.ToString("X16");

    public override string ToString() => Hex;

    public override bool Equals(object? obj)
    {
        return obj is TitleId other && Num == other.Num;
    }

    public override int GetHashCode() => Num.GetHashCode();
}
