# Azure Function: User Intent Classification

This project defines an Azure Function application for classifying user intent. The function analyzes user queries to determine the appropriate database for querying based on the query's intent, choosing between SQLite, CosmosDB, and SQL Server.

## Features
- **Environment-based Configuration**: The application retrieves configuration parameters (API key, endpoint, deployment, and version) from environment variables.
- **User Intent Classification**: Uses an Azure OpenAI model to classify user queries and determine which database the query is associated with.
- **Error Handling**: Captures and logs errors related to configuration, model initialization, and intent classification.

## Project Structure
- `router` (Azure Function endpoint): Main route for handling HTTP requests and classifying user queries.
- `QuestionRouter` class: Pydantic model for validating and structuring classification results.
- `classify_intent`: Helper function for validating and cleaning model responses.
- `system` message: System prompt for guiding the language model (LLM) to classify user intent.
- **Dependencies**: 
    - `azure-functions`: For building and handling Azure HTTP-triggered functions.
    - `langchain_core`, `langchain_openai`: For creating and running the language model prompt sequences.
    - `pydantic`: For model validation.

## Prerequisites
1. Azure account with permissions to deploy and manage Azure Functions.
2. Azure Functions Core Tools installed.
3. Azure OpenAI GPT 3.5 Turbo model credentials.

## Setup and Installation

1. **Clone the Repository**:
    ```bash
    git clone https://github.com/dsantafe/ChatWithYourDataChallenge.git
    cd intention-router-api-python
    ```

2. **Set Up Environment Variables**:
    Ensure the following environment variables are configured in your file "local.settings.json":
    - `AZURE_OPENAI_API_KEY`: Your Azure OpenAI API Key.
    - `AZURE_OPENAI_ENDPOINT`: The endpoint URL for Azure OpenAI.
    - `AZURE_OPENAI_DEPLOY`: The deployment name for your OpenAI model.
    - `AZURE_OPENAI_VERSION`: The API version to use.

3. **Install Dependencies**:
    ```bash
    pip install -r requirements.txt
    ```

4. **Run Locally**:
    Use the Azure Functions Core Tools to run the function locally:
    ```bash
    func start
    ```

## Usage

1. Send a POST or GET request to the `/api/router` endpoint with a JSON payload that includes a `query` parameter.
2. Since `http_auth_level` is set to `func.AuthLevel.FUNCTION`, you must provide the function key as a query parameter (`code`).
3. The function will analyze the query and return a single-word classification, identifying which database to use (`sqlite`, `cosmosdb`, or `sqlserver`).

### Example Request

```json
POST /api/router?code=YOUR_FUNCTION_KEY
{
  "query": "What are the details of employee training?"
}
