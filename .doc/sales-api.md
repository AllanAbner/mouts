[Back to README](../README.md)

### Sales

#### GET /api/sales
- Description: Retrieve a paginated list of sales
- Query Parameters:
  - `_page` (optional): Page number for pagination
  - `_size` (optional): Number of items per page
  - `_order` (optional): Ordering of results
  - `minDate` (optional): Minimum sale date
  - `maxDate` (optional): Maximum sale date
  - `customerExternalId` (optional): Filter by customer
  - `branchExternalId` (optional): Filter by branch
  - `isCancelled` (optional): Filter by cancellation status

#### POST /api/sales
- Description: Create a sale with one or more items

#### GET /api/sales/{id}
- Description: Retrieve a sale by id

#### PUT /api/sales/{id}
- Description: Update an active sale
- Notes:
  - Requires `version` for optimistic concurrency
  - Cancelled sales cannot be updated
  - Cancelled items cannot be changed

#### DELETE /api/sales/{id}
- Description: Cancel a sale logically

#### PATCH /api/sales/{saleId}/items/{itemId}/cancel
- Description: Cancel a single sale item logically

### Rules
- Quantities below 4 items do not receive discount
- Quantities from 4 to 9 receive 10% discount
- Quantities from 10 to 20 receive 20% discount
- Quantities above 20 are rejected
- Cancelled items do not contribute to the sale total
- A sale must keep at least one active item
- Events are implemented as MediatR notifications handled via structured logging
