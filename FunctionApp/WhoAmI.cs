using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System;

namespace FunctionApp
{
    public static class WhoAmI
    {
        private static ActionResult UnauthorizedResult(string message)
        {
            var result = new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return result;
        }

        [FunctionName("WhoAmI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                if (req.Headers.TryGetValue("Authorization", out var authHeader) == false)
                {
                    return UnauthorizedResult("No Authorization header");
                }

                var token = authHeader.ToString().Replace("Bearer ", "");
                var jwt = new JwtSecurityToken(token);

                if (jwt.Payload.TryGetValue("email", out object email))
                {
                    return new OkObjectResult($"Hello, {email}");
                }
                else
                {
                    return UnauthorizedResult("No email claim");
                }
            }
            catch (ArgumentException)
            {
                return UnauthorizedResult("Invalid token");
            }
            catch
            {
                return new UnauthorizedResult();
            }
        }
    }
}
