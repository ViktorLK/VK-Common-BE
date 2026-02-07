using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace VK.Lab.LayeredArchitecture.Filters
{
    /// <summary>
    /// グローバル例外フィルター - すべての未処琁E��外をキャチE��して統一皁E��エラーレスポンスを返す
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GlobalExceptionFilter(
            ILogger<GlobalExceptionFilter> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 例外発生時に実行される
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var request = context.HttpContext.Request;

            // FluentValidation の ValidationException を特別に処琁E
            if (exception is ValidationException validationException)
            {
                _logger.LogWarning(validationException,
                    "Validation failed. Path: {Path}, Method: {Method}",
                    request.Path,
                    request.Method);

                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Validation failed",
                    Errors = errors,
                    Path = request.Path.Value,
                    Method = request.Method,
                    Timestamp = DateTime.UtcNow
                });

                context.ExceptionHandled = true;
                return;
            }

            // エラーログを記録
            _logger.LogError(exception,
                "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                request.Path,
                request.Method);

            // HTTP スチE�Eタスコードを決宁E
            var statusCode = DetermineStatusCode(exception);

            // エラーレスポンスを構篁E
            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = GetErrorMessage(exception),
                Path = request.Path.Value,
                Method = request.Method,
                Timestamp = DateTime.UtcNow,
                // 開発環墁E��のみスタチE��トレースを含める
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            };

            // レスポンスを設宁E
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = (int)statusCode
            };

            // 例外を処琁E��みとしてマ�Eク
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 例外�E型に基づぁE��適刁E�� HTTP スチE�Eタスコードを決宁E
        /// </summary>
        private static HttpStatusCode DetermineStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }

        /// <summary>
        /// エラーメチE��ージを取得（本番環墁E��は一般皁E��メチE��ージを返す�E�E
        /// </summary>
        private string GetErrorMessage(Exception exception)
        {
            // 本番環墁E��は詳細なエラーメチE��ージを隠ぁE
            if (!_environment.IsDevelopment())
            {
                return exception switch
                {
                    ArgumentNullException => "Invalid request parameters",
                    ArgumentException => "Invalid request parameters",
                    InvalidOperationException => "Invalid operation",
                    KeyNotFoundException => "Resource not found",
                    UnauthorizedAccessException => "Unauthorized access",
                    NotImplementedException => "Feature not implemented",
                    _ => "An error occurred processing your request"
                };
            }

            // 開発環墁E��は詳細なエラーメチE��ージを返す
            return exception.Message;
        }
    }
}
