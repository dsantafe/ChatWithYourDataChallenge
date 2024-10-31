namespace ChatWithYourData.WebApp.Controllers
{
    using ChatWithYourData.Application.DTOs;
    using ChatWithYourData.Application.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;

    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController(
        IUserTokenService userTokenService) : ControllerBase
    {
        /// <summary>
        /// Authenticates a user and generates user tokens based on the provided login credentials.
        /// </summary>
        /// <param name="request">An object containing the user's login credentials, including username and password.</param>
        /// <returns>
        /// Returns an <see cref="IResult"/> representing the outcome of the login attempt. 
        /// - If the login is successful, it returns a 200 OK status with the user's tokens.
        /// - If the credentials are invalid, it returns a 401 Unauthorized status.
        /// - If the request model is invalid, it returns a 400 Bad Request status with validation errors.
        /// </returns>
        [HttpPost]
        [Route("Login")]
        public IResult Login(LoginReqDTO request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UserTokensDTO userToken = userTokenService.GetUserTokens(request.Username, request.Password);
                    if (userToken != null)
                        return Results.Ok(userToken);

                    return Results.Unauthorized();
                }
                else
                    return Results.BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.Message : ex.InnerException?.Message;
                return Results.Json(new ResponseDTO { Message = message }, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
