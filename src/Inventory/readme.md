# Inventory

The inventory service manages the availability of products.

## Features

### Requests

#### Restock Inventory Item

**Endpoint:** `POST /inventory/items/{productSlug}/restock`

Users can restock inventory items, increasing the quantity-in-stock of the restocked product.

#### Check Quantity In Stock

**Endpoint:** `GET /inventory/items/{productSlug}/quantity`

Users can check the quantity in stock for a particular product

### Event Consumers

#### Product Created (Published by "Products")

When a new product is created, it is automatically registered in the inventory system with a quantity-in-stock of 0.
This ensures that the inventory always has a record of all known products in the system.

