using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VK.Blocks.Core;

namespace VK.Blocks.Web.Shaping.Internal;

/// <summary>
/// A filter that shapes the response data based on the 'fields' query parameter.
/// Supports only <see cref="VKApiResponse{T}"/> or <see cref="VKPagedResponse{T}"/>.
/// </summary>
public sealed class ResponseShapingFilter : IAsyncResultFilter
{
    private static readonly ConcurrentDictionary<(Type Type, string Fields), Func<object, Dictionary<string, object?>>> _shaperCache = new();

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        var fieldsQuery = context.HttpContext.Request.Query["fields"].ToString();

        if (string.IsNullOrWhiteSpace(fieldsQuery) || context.Result is not ObjectResult objectResult)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var fields = fieldsQuery.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(f => f.Trim())
                                .OrderBy(f => f) // Ensure consistent key for cache
                                .ToArray();

        if (fields.Length == 0)
        {
            await next().ConfigureAwait(false);
            return;
        }

        objectResult.Value = ShapeValue(objectResult.Value, fields);

        await next().ConfigureAwait(false);
    }

    private object? ShapeValue(object? value, string[] fields)
    {
        VKGuard.NotNull(fields);

        if (value is null)
            return null;

        var type = value.GetType();

        if (!type.IsGenericType)
            return value;

        var genericTypeDefinition = type.GetGenericTypeDefinition();

        if (genericTypeDefinition == typeof(VKApiResponse<>))
        {
            var dataProp = type.GetProperty("Data");
            var dataValue = dataProp?.GetValue(value);
            if (dataValue != null)
            {
                return new
                {
                    Success = (bool)(type.GetProperty("Success")?.GetValue(value) ?? true),
                    Data = ShapeData(dataValue, fields),
                    VKError = type.GetProperty("VKError")?.GetValue(value)
                };
            }
        }
        else if (genericTypeDefinition == typeof(VKPagedResponse<>))
        {
            var itemsProp = type.GetProperty("Items");
            var itemsValue = itemsProp?.GetValue(value);
            if (itemsValue != null)
            {
                return new
                {
                    Success = (bool)(type.GetProperty("Success")?.GetValue(value) ?? true),
                    Items = ShapeData(itemsValue, fields),
                    PageNumber = (int)(type.GetProperty("PageNumber")?.GetValue(value) ?? 0),
                    PageSize = (int)(type.GetProperty("PageSize")?.GetValue(value) ?? 0),
                    TotalCount = (int)(type.GetProperty("TotalCount")?.GetValue(value) ?? 0),
                    TotalPages = (int)(type.GetProperty("TotalPages")?.GetValue(value) ?? 0),
                    VKError = type.GetProperty("VKError")?.GetValue(value)
                };
            }
        }

        return value;
    }

    private object? ShapeData(object data, string[] fields)
    {
        VKGuard.NotNull(data);
        VKGuard.NotNull(fields);

        if (data is IEnumerable<object> list)
        {
            return list.Select(item => ShapeObject(item, fields)).ToList();
        }

        return ShapeObject(data, fields);
    }

    private Dictionary<string, object?> ShapeObject(object obj, string[] fields)
    {
        VKGuard.NotNull(obj);
        VKGuard.NotNull(fields);

        var type = obj.GetType();
        var fieldsKey = string.Join(",", fields);

        var shaper = _shaperCache.GetOrAdd((type, fieldsKey), static key => CompileShaper(key.Type, key.Fields.Split(',')));

        return shaper(obj);
    }

    private static Func<object, Dictionary<string, object?>> CompileShaper(Type type, string[] fields)
    {
        var inputParam = Expression.Parameter(typeof(object), "obj");
        var typedParam = Expression.Convert(inputParam, type);
        var dictType = typeof(Dictionary<string, object?>);
        var addMethod = dictType.GetMethod("Add")!;

        var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var elementInits = new List<ElementInit>();
        foreach (var field in fields)
        {
            var prop = allProps.FirstOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                elementInits.Add(Expression.ElementInit(
                    addMethod,
                    Expression.Constant(prop.Name),
                    Expression.Convert(Expression.Property(typedParam, prop), typeof(object))
                ));
            }
        }

        var body = Expression.ListInit(
            Expression.New(dictType),
            elementInits
        );

        return Expression.Lambda<Func<object, Dictionary<string, object?>>>(body, inputParam).Compile();
    }
}
