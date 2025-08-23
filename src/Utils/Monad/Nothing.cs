namespace Utils.Monad;

/// <summary>
/// Represents the absence of a value. 
/// Can be used in generic contexts where a type parameter is required, 
/// but no actual data needs to be stored.
/// </summary>
public readonly record struct Nothing;