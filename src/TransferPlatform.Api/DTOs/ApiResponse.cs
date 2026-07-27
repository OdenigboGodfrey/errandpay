using Microsoft.AspNetCore.Mvc;

namespace TransferPlatform.src.TransferPlatform.Api.DTOs
{

    public class ApiResponse<T>
    {
        public string Code { get; set; } = "400";
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public static class ResponseHelper
    {
        public static IActionResult BuildResponse<T>(
            string code,
            bool status,
            string message,
            T? data)
        {
            var response = new ApiResponse<T>
            {
                Status = status,
                Message = message,
                Data = data
            };

            return code switch
            {
                "200" => new OkObjectResult(response),

                "201" => new CreatedResult(string.Empty, response),

                "400" => new BadRequestObjectResult(response),

                "401" => new UnauthorizedObjectResult(response),

                "403" => new ObjectResult(response) { StatusCode = 403 },

                "404" => new NotFoundObjectResult(response),

                "500" => new ObjectResult(response) { StatusCode = 500 },

                _ => new ObjectResult(response) { StatusCode = 200 }
            };
        }
    }
}