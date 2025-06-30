# Catalog

The catalog "slice" manages the products available in the system.

## Features

### Requests

#### Create Product

**Endpoint:** `POST /products`

Users can create new products with a unique slug, name, and optional attributes. The slug serves as a URL-friendly identifier and cannot be changed once the product is created.

#### Get Product

**Endpoint:** `GET /products/{identifier}`

Users can retrieve product details by either the product ID or slug. This provides flexibility in how products are accessed and referenced.

#### List Products

**Endpoint:** `GET /products`

Users can retrieve a paginated list of all products in the system. The query supports offset and limit parameters for efficient pagination.
