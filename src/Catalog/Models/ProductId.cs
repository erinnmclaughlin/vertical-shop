namespace VerticalShop.Catalog;

/// <summary>
/// A unique identifier for a product, encapsulating its creation, parsing, and conversion functionalities.
/// </summary>
public sealed record ProductId(Guid Value)
{
    /// <summary>
    /// Creates a new, unique <see cref="ProductId"/> instance using a version 7 GUID.
    /// </summary>
    /// <returns>A unique <see cref="ProductId"/> instance.</returns>
    public static ProductId CreateNew() => new(Guid.CreateVersion7());
    
    /// <summary>
    /// Parses the specified string value into a new <see cref="ProductId"/> instance.
    /// </summary>
    /// <param name="value">The string representation of the product ID to parse.</param>
    /// <returns>A <see cref="ProductId"/> instance with the specified value.</returns>
    public static ProductId Parse(string value) => new(Guid.Parse(value));

    /// <summary>
    /// Returns the string representation of the <see cref="ProductId"/>.
    /// </summary>
    /// <returns>The unique identifier as a string.</returns>
    public override string ToString() => Value.ToString();
}
