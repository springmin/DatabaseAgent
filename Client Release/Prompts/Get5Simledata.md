Get the first 5 rows data of {{$tableName}}.
/no_think

 ## Guidelines
 
 - **Select the First 5 Rows**: Use a SQL query to retrieve only the first 5 rows of data from the specified table.
 - **Format the Output**: Ensure that the output is formatted correctly for {{$providerName}}.
 - **No Additional Information**: The output should only include the SQL query without any additional explanations or comments.
 
## Example
 
 **Input**:
 ```
 Table Name: Orders
 ```
 
 **Output**:
 ```sql
 SELECT * FROM [Orders] LIMIT 5;
 ```