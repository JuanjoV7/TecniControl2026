namespace TecniControl2026.Web.Core
{
    public class Response<TResult>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public TResult? Result { get; set; }

        public static Response<TResult> Failure(Exception ex, string message = "Ha ocurrido un error al generar la solicitud")
        {
            return new Response<TResult>
            {
                IsSuccess = false,
                Message = message,

                // TODO: Remove in production, only for debugging purposes
                Errors = new List<string>
                {
                    ex.Message
                }
            };
        }

        public static Response<TResult> Failure(string message, List<string>? errors = null)
        {
            return new Response<TResult>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }

        public static Response<TResult> Success(TResult result, string message = "Tarea realizada con éxito")
        {
            return new Response<TResult>
            {
                IsSuccess = true,
                Message = message,
                Result = result
            };
        }

        public static Response<TResult> Success(string message = "Tarea realizada con éxito")
        {
            return new Response<TResult>
            {
                IsSuccess = true,
                Message = message,
            };
        }
    }
}
