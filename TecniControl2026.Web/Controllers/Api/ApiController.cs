using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TecniControl2026.Web.Core;

namespace TecniControl2026.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        public static ObjectResult ControllerBasicValidation<T>(Response<T> response,
                                                                ModelStateDictionary? modelState = null,
                                                                int? statusCode = null)
        {
            if (modelState is not null && !modelState.IsValid)
            {
                List<string> errors = modelState.Values.SelectMany(v => v.Errors)
                                                       .Select(e => e.ErrorMessage)
                                                       .ToList();

                return new ObjectResult(Response<T>.Failure("Debe ajustar los errores de validación"))
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
            }

            if (statusCode is not null && statusCode >= 100 && statusCode <= 599)
            {
                return new ObjectResult(response)
                {
                    StatusCode = statusCode
                };
            }

            if (!response.IsSuccess)
            {
                return new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}