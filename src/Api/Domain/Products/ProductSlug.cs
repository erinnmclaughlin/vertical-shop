namespace ContextDrivenDevelopment.Api.Domain.Products;

/// <summary>
/// A unique identifier for a Product in the form of a slug.
/// </summary>
public sealed record ProductSlug
{
    /// <summary>
    /// The unique slug value representing the identifier for a Product.
    /// </summary>
    public string Value { get; private init; }

    private ProductSlug()
    {
        Value = Guid.CreateVersion7().ToString();
    }

    /// <summary>
    /// Creates a new instance of the ProductSlug class with a unique value.
    /// </summary>
    /// <returns>A new instance of ProductSlug with a generated unique identifier.</returns>
    public static ProductSlug CreateNew() => new();

    /// <summary>
    /// Parses the provided string and creates a new instance of the ProductSlug class with the specified value.
    /// </summary>
    /// <param name="value">The string representation of the product slug to parse.</param>
    /// <returns>A new instance of ProductSlug initialized with the specified value.</returns>
    public static ProductSlug Parse(string value) => new() { Value = value };

    /// <summary>
    /// Defines an implicit conversion operator to convert a ProductSlug instance to its string representation.
    /// </summary>
    /// <param name="slug">The ProductSlug instance to be converted to a string.</param>
    /// <returns>The string representation of the product slug.</returns>
    public static implicit operator string(ProductSlug slug) => slug.Value;

    /// <summary>
    /// Returns the string representation of the ProductSlug instance.
    /// </summary>
    /// <returns>The string value of the ProductSlug.</returns>
    public override string ToString() => Value;
}