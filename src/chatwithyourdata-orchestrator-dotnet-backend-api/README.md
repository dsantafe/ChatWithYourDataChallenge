# Chat With Your Data

This project provides a robust API interface to interact with employee-related data through natural language queries. It leverages Azure OpenAI, CosmosDB, SQL Server, and SQLite, orchestrating interactions for employee information, attendance, training sessions, evaluations, and feedback.

## Table of Contents
- [Project Overview](#project-overview)
- [Prerequisites](#prerequisites)
- [Environment Variables](#environment-variables)
- [Setup and Installation](#setup-and-installation)
- [Running the Project](#running-the-project)
- [API Documentation](#api-documentation)

## Project Overview
The system allows users to query employee-related data using natural language. The backend interprets the query, routes it to the correct database, and provides a response. Key components include:
- **Azure OpenAI**: Generates SQL queries from natural language inputs.
- **Database Router**: Determines the appropriate data source for each query.
- **Swagger API Documentation**: Provides API interaction documentation.
- **Postman Collection**: Predefined API requests for testing and development.

## Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (latest version)
- [Azure account](https://azure.microsoft.com/) with OpenAI and CosmosDB resources
- [SQL Server](https://www.microsoft.com/en-us/sql-server) and [SQLite](https://www.sqlite.org/index.html) for local database setup

## Environment Variables
Configure these variables in your development environment:

```json
"AppSettings": {
  "USER_TOKENS_AVAILABLE": 10000,
  "CHAT_WITH_YOUR_DATA_CONNECTION_STRING": "Server=<servername>.database.windows.net;Database=<databasename>;User Id=<username>; Password=<password>;",
  "OPENAI_RETAIL_PRICES": "https://prices.azure.com/api/retail/prices",
  "OPENAI_ROUTER_API": "<deployment llm router intention>",
  "OPENAI_DEPLOYMENT_NAME": "<model name deployed>",
  "OPENAI_ENDPOINT_URL": "<url>",
  "OPENAI_API_KEY": "<key>",
  "MSSQL_CONNECTION_STRING": "Server=<servername>.database.windows.net;Database=<databasename>;User Id=<username>; Password=<password>;",
  "COSMOS_CONNECTION_STRING": "AccountEndpoint=<servername>;AccountKey=<key>",
  "COSMOS_DATABASE": "<databasename>",
  "COSMOS_CONTAINER": "<container>",
  "SQLITE_DATABASE_URL": "https://<databasename>-<username>.turso.io",
  "SQLITE_AUTH_TOKEN": "<token>"
}
```



Replace placeholder values (`YOUR_OPENAI_API_KEY`, `YOUR_SERVER`, `YOUR_USER_ID`, `YOUR_PASSWORD`, `YOUR_COSMOS_KEY`, and `YOUR_SQLITE_AUTH_TOKEN`) with your actual credentials.

## Setup and Installation
1. **Clone the repository**:
   ```bash
   git clone https://github.com/dsantafe/ChatWithYourDataChallenge.git
   cd src/orchestrator-backend-api-dotnet
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Configure environment variables**:
   Set up the environment variables listed above in your `.env` file or directly in your development environment.

## Running the Project
1. **Run the application**:
   ```bash
   dotnet run
   ```

2. **Access the API**:
   - Open the Swagger UI to explore and test endpoints: [Swagger Documentation](https://chat-with-your-data-api.azurewebsites.net/swagger/index.html)
   - Alternatively, you can use the Postman collection for predefined API requests: [Postman Documentation](https://documenter.getpostman.com/view/3923266/2sAY4uCiV6#99522633-09e9-4e29-86db-3fb59f6c4781)

## API Documentation
- **Swagger UI**: The API endpoints are documented and can be tested interactively [here](https://chat-with-your-data-api.azurewebsites.net/swagger/index.html).
- **Postman Collection**: Import and use the [Postman Collection](https://documenter.getpostman.com/view/3923266/2sAY4uCiV6#99522633-09e9-4e29-86db-3fb59f6c4781) for testing all endpoints in Postman.

## Links
- Turso - Databases for All:  https://turso.tech/
- Azure Cosmos DB emulator: https://aka.ms/cosmosdb-emulator