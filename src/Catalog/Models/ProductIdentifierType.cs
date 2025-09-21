using System.Text.Json.Serialization;

namespace VerticalShop.Catalog;

/// <summary>
/// The types of product identifiers that can be used to uniquely identify a product.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductIdentifierType
{
    /// <summary>
    /// The product is identified by its unique identifier (ID).
    /// </summary>
    Id,
    
    /// <summary>
    /// The product is identified by its URL-friendly slug.
    /// </summary>
    Slug
}