# Inventory

The inventory service manages the availability of products.

## Features

### Commands

#### Restock Inventory Item

A worker can restock an inventory item, which increases the quantity-in-stock of the restocked product.

### Event Consumers

#### Product Created

When a new product is created, it is automatically registered in the inventory system with a quantity-in-stock of 0.
This ensures that the inventory always has a record of all known products in the system.

