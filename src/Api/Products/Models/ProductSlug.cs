namespace VerticalShop.Api.Products;

/// <summary>
/// A unique identifier for a Product in the form of a slug.
/// </summary>
public sealed record ProductSlug
{
    /// <summary>
    /// The unique slug value representing the identifier for a Product.
    /// </summary>
    public string Value { get; private init; }

    private ProductSlug() : this(Guid.CreateVersion7().ToString())
    {
    }
    
    private ProductSlug(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ProductSlug"/> class with a unique value.
    /// </summary>
    /// <returns>A new instance of <see cref="ProductSlug"/> with a generated unique identifier.</returns>
    public static ProductSlug CreateNew() => new();

    /// <summary>
    /// Parses the provided string and creates a new instance of the <see cref="ProductSlug"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>A new instance of ProductSlug initialized with the specified value.</returns>
    public static ProductSlug Parse(string value)
    {
        // TODO: Validate slug format
        return new ProductSlug(value);
    }

    /// <summary>
    /// Defines an implicit conversion operator to convert a <see cref="ProductSlug"/> instance to its string representation.
    /// </summary>
    /// <param name="slug">The <see cref="ProductSlug"/> instance to be converted to a string.</param>
    /// <returns>The string representation of the product slug.</returns>
    public static implicit operator string(ProductSlug slug) => slug.Value;

    /// <summary>
    /// Defines an implicit conversion operator to convert a string to a <see cref="ProductSlug"/> instance.
    /// </summary>
    /// <param name="slug">The string to be converted into a <see cref="ProductSlug"/> instance.</param>
    /// <returns>A <see cref="ProductSlug"/> initialized with the specified string value.</returns>
    public static implicit operator ProductSlug(string slug) => Parse(slug);

    /// <summary>
    /// Returns the string representation of the <see cref="ProductSlug"/> instance.
    /// </summary>
    /// <returns>The string value of the <see cref="ProductSlug"/>.</returns>
    public override string ToString() => Value;
}