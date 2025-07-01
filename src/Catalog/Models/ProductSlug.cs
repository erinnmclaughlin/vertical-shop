using System.Diagnostics.CodeAnalysis;

namespace VerticalShop.Catalog;

/// <summary>
/// A unique identifier for a Product in the form of a slug.
/// </summary>
public sealed record ProductSlug
{
    /// <summary>
    /// The unique slug value representing the identifier for a Product.
    /// </summary>
    public string Value { get; }

    private ProductSlug(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Attempts to parse the provided string and create a new instance of the <see cref="ProductSlug"/> class,
    /// if the value is in a valid format.
    /// </summary>
    /// <param name="value">The string value to attempt to parse.</param>
    /// <param name="slug">
    /// When this method returns, contains the created <see cref="ProductSlug"/> instance if parsing succeeded,
    /// or null if parsing failed.
    /// </param>
    /// <returns>
    /// true if the parsing was successful and a valid <see cref="ProductSlug"/> instance was created;
    /// otherwise, false.
    /// </returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out ProductSlug? slug)
    {
        slug = IsValidSlug(value) ? new ProductSlug(value) : null;
        return slug is not null;
    }

    /// <summary>
    /// Parses the provided string and creates a new instance of the <see cref="ProductSlug"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>A new instance of ProductSlug initialized with the specified value.</returns>
    public static ProductSlug Parse(string value)
    {
        if (!TryParse(value, out var slug))
            throw new ArgumentException("Invalid slug format.", nameof(value));
        
        return slug;
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
    
    private static bool IsValidSlug(string value)
    {
        // Slug should only contain lowercase letters, numbers, and hyphens
        // It should not start or end with a hyphen
        return !string.IsNullOrWhiteSpace(value) &&
               !value.StartsWith('-') 
               && !value.EndsWith('-') 
               && value.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

}