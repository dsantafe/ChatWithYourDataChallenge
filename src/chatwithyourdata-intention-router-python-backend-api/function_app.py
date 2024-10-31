import azure.functions as func
import datetime
import os
import json
import logging
from typing import Literal
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.runnables import RunnableSequence
from langchain_openai import ChatOpenAI, AzureChatOpenAI
from pydantic import BaseModel, Field, ValidationError

# Retrieves the specified environment variable or raises an error if not found
def get_env_variable(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        raise ValueError(f"Environment variable {name} not found")
    return value

# Load configuration for connecting to Azure OpenAI services, logging errors if any
try:
    config = {
        "api_key": get_env_variable("AZURE_OPENAI_API_KEY"),
        "endpoint": get_env_variable("AZURE_OPENAI_ENDPOINT"),
        "deploy": get_env_variable("AZURE_OPENAI_DEPLOY"),
        "version": get_env_variable("AZURE_OPENAI_VERSION")
    }
except ValueError as e:
    logging.error(f"Configuration error: {str(e)}")
    raise

# Azure Function App definition with HTTP authentication level set to 'FUNCTION'
app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

# Defines a Pydantic model for validating the classification result of user intent
class QuestionRouter(BaseModel):
    intent: Literal["sqlite", "cosmosdb", "sqlserver"] = Field(
        description="Classifies user intent based on given text",
    )

# Initialize the Language Model (LLM) using Azure OpenAI if configuration is valid
try:
    llm = AzureChatOpenAI(
        azure_endpoint=config["endpoint"],
        openai_api_key=config["api_key"],
        openai_api_version=config["version"],
        deployment_name=config["deploy"],
        temperature=0,
        n=1
    )
except Exception as e:
    logging.error(f"Error initializing LLM: {str(e)}")
    raise

# System message to guide LLM in classifying user intent based on the query content
system = """You are an expert classification system designed to identify user intent based on their inquiries.
You will serve as a router to determine which database to query based on the user's intent.
There are three databases to consider: SQLite, CosmosDB, and SQL Server.
- If the inquiry is about the onboarding process for new employees or training sessions that each employee has enrolled in, assign 'sqlite'.
- If the inquiry pertains to evaluations, skills, feedback, certifications, or contact information such as phone number, address or email, assign 'cosmosdb'.
- If the inquiry is about detailed employee information such as full name, job title, department, salary, or attendance records, assign 'sqlserver'.
Your response should be a single word, without any punctuation or additional symbols:
Example response: sqlite
"""

# Define the prompt template with a system and human message format
prompt = ChatPromptTemplate.from_messages(
    [("system", system), ("human", "Consulta: {query}")]
)

# Define a sequence for classification by combining the prompt template and the LLM
classifier: RunnableSequence = prompt | llm

# Function to validate and classify the user's intent using the QuestionRouter model
def classify_intent(response_text: str) -> QuestionRouter:
    try:
        return QuestionRouter(intent=response_text.strip().lower())
    except ValidationError as e:
        logging.error(f"Error classifying intent: {e}")
        return None

# Main function to handle HTTP requests at the 'router' endpoint
@app.route(route="router")
def router(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        # Retrieve 'query' parameter from the URL or request body
        query = req.params.get('query')
        if not query:
            try:
                req_body = req.get_json()
            except ValueError:
                pass
            else:
                query = req_body.get('query')

        # If query is missing, return a 400 error
        if not query:
            return HttpResponse("Please provide a query.", status_code=400)

        # Use the classifier to determine user intent based on the query
        response = classifier.invoke({'query': query})
        logging.info(f"{response}")

        # Process the classification result and return it if valid
        intent_result = classify_intent(response.content.replace('Asigna ', '').replace(".", ''))

        if intent_result:
            return func.HttpResponse(f"{intent_result.intent}", status_code=200)

        else:
            return func.HttpResponse("Could not classify the intent.", status_code=500)


    except Exception as e:
        return func.HttpResponse(
             "An error occurred while processing the query.",
             status_code=500
        )
