Please write a SQL query based on the provided information, including the DBMS type, the natural language description, and the table/column definitions.

Your task is to generate a SQL query aligned with the syntax of the specified DBMS provider, ensuring it is optimized and syntactically correct. Please adhere to the following steps:

1. **Parse Input:**
   - Identify the DBMS provider from the input (e.g., MySQL, PostgreSQL, SQL Server, SQLite, etc.) and consider its specific syntax or features.
   - Extract the natural language query asking for the data.
   - Review the table and column definitions, understanding relationships between tables, primary/foreign keys, column data types, and any constraints.

2. **Generate Query:**
   - Transform the natural language description into a SQL query, adhering to the specified DBMS syntax.
   - Use JOIN operations, filters, aggregations (e.g., GROUP BY), and conditions (e.g., WHERE, HAVING) as required by the query.

3. **Verify Correctness:**
   - Ensure the query is valid for the described schema, using the exact table and column names provided.
   - Prioritize clarity and optimization for the specified DBMS.

## Important

You should only use SQL syntax that is compatible with the specified DBMS provider. If the provider is not specified, assume standard SQL syntax.
Ensure that all table names are complete and properly formatted for use in SQL queries.
If a table name contains spaces, special characters, or starts with a number, enclose it in square brackets [] (e.g., [Table Name]).
Do not modify names that are already correctly bracketed. The formatting should be compatible with {{$providerName}}

# Output Format 

```json
{
  "query": "SELECT ... FROM ... WHERE ...",
  "comments": [
    "Assumptions made about the query structure.",
    "Any specific optimizations or considerations for the DBMS."
  ]}
```

# Examples

### Example 1

#### Input:
**DBMS Provider:** MySQL
**Natural Language Query:** "Get the names and emails of all users who registered after January 1, 2023."
**Tables and Columns:**
  - `users`: 
    - `id`: INT (Primary Key)
    - `name`: VARCHAR
    - `email`: VARCHAR
    - `registered_date`: DATE

#### Output:

```json
{
  "query": "SELECT name, email FROM users WHERE registered_date > '2023-01-01'",
  "comments": [
    "Assumes 'registered_date' is stored in a DATE or DATETIME format compatible with the string '2023-01-01'.",
    "Indexes on 'registered_date' can significantly improve performance for large datasets."
  ]
```

---

### Example 2

#### Input:
**DBMS Provider:** PostgreSQL
**Natural Language Query:** "List the total sales per product, including the product name, for products with sales exceeding $1000."
**Tables and Columns:**
  - `products`:
    - `id`: INT (Primary Key)
    - `name`: VARCHAR
  - `sales`:
    - `id`: INT (Primary Key)
    - `product_id`: INT (Foreign Key to products.id)
    - `amount`: NUMERIC

#### Output:
```json
{
  "query": "SELECT p.name, SUM(s.amount) AS total_sales FROM products p JOIN sales s ON p.id = s.product_id GROUP BY p.name HAVING SUM(s.amount) > 1000;",
  "comments": [
    "Assumes each sale in the 'sales' table is linked to a product via 'product_id'.",
    "Using HAVING instead of WHERE because aggregate function SUM(s.amount) is used for filtering.",
    "Ensure proper indexing on 'sales.product_id' and possibly 'sales.amount' for better performance.",
    "GROUP BY on 'p.name' may lead to grouping issues if product names are not unique; consider using 'p.id' instead for more accuracy."
  ]}
```

Use placeholders like [DBMS], [natural language query], and [table definitions] where details are not provided explicitly, and adapt the examples for the information given.

## Let's do it for real

#### Input:
**DBMS Provider:** {{$providerName}}
**Natural Language Query:** "{{$prompt}}"
**Tables and Columns:**
{{$tablesDefinition}}

#### Output: