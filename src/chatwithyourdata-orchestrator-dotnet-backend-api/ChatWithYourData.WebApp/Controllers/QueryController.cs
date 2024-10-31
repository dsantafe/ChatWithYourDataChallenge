namespace ChatWithYourData.WebApp.Controllers
{
    using ChatWithYourData.Application.DTOs;
    using ChatWithYourData.Application.Interfaces;
    using ChatWithYourData.Domain.Enums;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;

    [ApiController]
    [Route("/api/[controller]")]
    public class QueryController(
        IWebHostEnvironment env,
        IUserTokenService userTokenService,
        INLToSQLQueryService queryService) : ControllerBase
    {
        private readonly string _templatesPath = Path.Combine(env.ContentRootPath, "Templates");

        /// <summary>
        /// Retrieves the token consumption history for a specified user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve token history for User.</param>
        /// <returns>
        /// An HTTP 200 OK response containing the user's token consumption history if successful,
        /// or an HTTP 400 Bad Request response if the userId is invalid.
        /// </returns>
        [HttpGet]
        [Route("GetTokensConsumptionHistory/{id}")]
        public IResult GetTokensConsumption(Guid id)
        {
            if (id == Guid.Empty)
                return Results.BadRequest();

            return Results.Ok(new ResponseDTO
            {
                IsSuccess = true,
                Data = userTokenService.GetTokensHistoryByUserId(id),
                Message = "Tokens consumption history retrieved successfully."
            });
        }

        /// <summary>
        /// Retrieves the token consumption details based on the provided natural language query.
        /// </summary>
        /// <param name="request">An object containing the natural language query for which token consumption is to be evaluated.</param>
        /// <returns>
        /// Returns an <see cref="IResult"/> representing the outcome of the token consumption request. 
        /// - If the model state is invalid, it returns a 400 Bad Request status with validation errors.
        /// - If the token consumption is successfully retrieved, it returns a 200 OK status with a success message and the token consumption details.
        /// </returns>
        [HttpPost]
        [Route("GetTokensConsumption")]
        public IResult GetTokensConsumption(TokensConsumptionReqDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Results.BadRequest(ModelState);

                return Results.Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Data = queryService.GetTokensConsumption(request.NaturalLanguageQuery, _templatesPath),
                    Message = "Token count validated successfully."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "InvalidDatabaseType")
                    return Results.BadRequest(ex.Message);
                string message = string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.Message : ex.InnerException?.Message;
                return Results.Json(new ResponseDTO { Message = message }, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Generates an SQL query based on the provided natural language query, subject to token consumption limits.
        /// </summary>
        /// <param name="request">An object containing the user's ID and the natural language query to be converted into SQL.</param>
        /// <returns>
        /// Returns an <see cref="IResult"/> representing the outcome of the SQL generation request.
        /// - If the model state is invalid, it returns a 400 Bad Request status with validation errors.
        /// - If the user tokens are invalid or insufficient, it returns a 401 Unauthorized status or a 400 Bad Request status with an error message.
        /// - If the SQL query is successfully generated, it returns a 200 OK status with the generated SQL and a success message.
        /// </returns>
        [HttpPost]
        [Route("GenerateSqlQuery")]
        public IResult GenerateSqlQuery(GenerateSqlQueryReqDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Results.BadRequest(ModelState);

                UserTokensDTO userTokens = userTokenService.GetUserTokensById(request.UserId);
                if (userTokens == null)
                    return Results.Unauthorized();

                DatabaseType databaseType = queryService.GetDatabaseType(request.SelectedDatabaseType);
                TokensConsumptionDTO responseTokens = queryService.GetTokensConsumption(
                    request.NaturalLanguageQuery,
                    databaseType,
                    _templatesPath);

                int tokenCount = responseTokens.TokenCount;
                if (tokenCount <= userTokens.TokensAvailable)
                {
                    GenerateSqlQueryDTO response = queryService.GenerateSqlQuery(
                        request.NaturalLanguageQuery,
                        databaseType,
                        _templatesPath);

                    userTokenService.UpdateTokenConsumption(request.UserId, tokenCount);

                    return Results.Ok(new ResponseDTO
                    {
                        IsSuccess = true,
                        Data = response,
                        Message = "SQL query generated successfully."
                    });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Insufficient tokens available.");
                    return Results.BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.Message : ex.InnerException?.Message;
                return Results.Json(new ResponseDTO { Message = message }, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes the provided SQL query and returns the results.
        /// </summary>
        /// <param name="request">An object containing the SQL query to be executed and the type of database.</param>
        /// <returns>
        /// Returns an <see cref="IResult"/> representing the outcome of the SQL execution request.
        /// - If the model state is invalid, it returns a 400 Bad Request status with validation errors.
        /// - If the execution is successful, it returns a 200 OK status with the query results and a success message.
        /// </returns>
        [HttpPost]
        [Route("ExecuteSqlQuery")]
        public IResult ExecuteSqlQuery(ExecuteSqlQueryReqDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Results.BadRequest(ModelState);

                DatabaseType databaseType = queryService.GetDatabaseType(request.SelectedDatabaseType);
                return Results.Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Data = queryService.ExecuteSqlQuery(request.GeneratedSqlQuery, databaseType),
                    Message = "SQL query executed successfully."
                });
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.Message : ex.InnerException?.Message;
                return Results.Json(new ResponseDTO { Message = message }, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
