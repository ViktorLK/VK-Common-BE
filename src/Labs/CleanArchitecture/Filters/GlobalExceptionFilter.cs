using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace VK.Lab.CleanArchitecture.Filters
{
    /// <summary>
    /// グローバル例外フィルター - すべての未処理例外をキャッチして統一的なエラーレスポンスを返す
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

            // FluentValidation の ValidationException を特別に処理
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

            // HTTP ステータスコードを決定
            var statusCode = DetermineStatusCode(exception);

            // エラーレスポンスを構築
            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = GetErrorMessage(exception),
                Path = request.Path.Value,
                Method = request.Method,
                Timestamp = DateTime.UtcNow,
                // 開発環境でのみスタックトレースを含める
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            };

            // レスポンスを設定
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = (int)statusCode
            };

            // 例外を処理済みとしてマーク
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 例外の型に基づいて適切な HTTP ステータスコードを決定
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
        /// エラーメッセージを取得（本番環境では一般的なメッセージを返す）
        /// </summary>
        private string GetErrorMessage(Exception exception)
        {
            // 本番環境では詳細なエラーメッセージを隠す
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

            // 開発環境では詳細なエラーメッセージを返す
            return exception.Message;
        }
    }
}
