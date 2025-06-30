namespace VerticalShop.Catalog;

/// <summary>
/// A unique identifier for a product, encapsulating its creation, parsing, and conversion functionalities.
/// </summary>
public sealed record ProductId
{
    /// <summary>
    /// The unique string value representing the identifier of a product.
    /// </summary>
    public string Value { get; private init; }

    private ProductId()
    {
        Value = Guid.CreateVersion7().ToString();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProductId"/> class with a unique identifier.
    /// </summary>
    /// <returns>A new <see cref="ProductId"/> instance containing a generated unique identifier.</returns>
    public static ProductId CreateNew() => new();

    /// <summary>
    /// Parses the specified string value into a new <see cref="ProductId"/> instance.
    /// </summary>
    /// <param name="value">The string representation of the product ID to parse.</param>
    /// <returns>A <see cref="ProductId"/> instance with the specified value.</returns>
    public static ProductId Parse(string value) => new() { Value = value };

    /// <summary>
    /// Performs an implicit conversion from a <see cref="ProductId"/> to its string representation.
    /// </summary>
    /// <param name="id">The <see cref="ProductId"/> instance to convert.</param>
    /// <returns>The string representation of the specified <see cref="ProductId"/>.</returns>
    public static implicit operator string(ProductId id) => id.Value;

    /// <summary>
    /// Performs an implicit conversion from a string to a <see cref="ProductId"/>.
    /// </summary>
    /// <param name="id">The string representation of the product ID to convert.</param>
    /// <returns>A new <see cref="ProductId"/> instance created from the specified string.</returns>
    public static implicit operator ProductId(string id) => Parse(id);
    
    /// <summary>
    /// Returns the string representation of the <see cref="ProductId"/>.
    /// </summary>
    /// <returns>The unique identifier as a string.</returns>
    public override string ToString() => Value;
}