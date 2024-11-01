# Chat with Your Data through DataVerse

This Streamlit app, "Chat with Your Data through DataVerse," enables users to interact with their data through natural language queries. The app connects to a backend API to retrieve token usage, generate SQL queries, and execute these queries in real time. 

## Features
- **Natural Language Query**: Transform natural language inputs into SQL queries.
- **Token Usage Tracking**: See token usage and cost estimations before query execution.
- **Data Display**: Show query results in a table with options to view SQL code.
- **Customizable UI**: Includes custom CSS for a modern interface.

---

## Requirements
- **Python 3.7 or higher**
- **Streamlit library**
- **Pandas**
- **Requests library**
- **Pillow for image processing**

## Installation

1. **Clone this repository**:
    ```bash
    git clone https://github.com/yourusername/chat-with-your-data.git
    cd chat-with-your-data
    ```

2. **Install dependencies**:
    ```bash
    pip install -r requirements.txt
    ```

3. **Set up your `.env` file** (Optional but recommended for secure API keys):
   - Create a `.env` file in the project root.
   - Add environment variables for sensitive data.

4. **Run the Streamlit App**:
    ```bash
    streamlit run app.py
    ```

---

## How It Works

### Custom Styling
This app includes a custom CSS function to provide a sleek and modern design with enhanced user interactivity. It applies gradients, animations, and background styling to improve usability.

### Functionality Overview

1. **Token Consumption Estimate**:
   - Uses the `get_token_comsuption` function to call the API and retrieve token count, database type, and estimated cost for a query.
   
2. **SQL Query Generation**:
   - The `generate_sql_query` function sends a natural language prompt to the API and retrieves an SQL query if tokens are available.

3. **SQL Query Execution**:
   - The `execute_sql_query` function calls the API with the SQL query and displays the resulting data table in the Streamlit app.

### Key Components

- **Custom CSS Styling**: Integrated for a user-friendly interface.
- **API Endpoints**: 
  - `/api/Query/GetTokensConsumption`
  - `/api/Query/GenerateSqlQuery`
  - `/api/Query/ExecuteSqlQuery`
- **Token Management**: Tracks tokens used per session and calculates total costs.

---

## Example Usage

1. Launch the app.
2. Enter a natural language query (e.g., “What is the sales report for the last quarter?”).
3. Confirm the token usage estimate.
4. Receive the generated SQL query and execute it to retrieve results.

## Additional Information

Refer to [Streamlit’s documentation](https://docs.streamlit.io/develop/tutorials/llms/build-conversational-apps) for more about building conversational apps and customizing layouts.

---

## File Structure
```plaintext
.
├── app.py                   # Main Streamlit app file
├── assets/
│   └── DataVerse.png        # Logo image for the sidebar
├── README.md                # Documentation
└── requirements.txt         # Dependencies for the project
