using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CITAssignment3;

public class RequestValidator
{
    private static readonly HashSet<string> ValidMethods = new()
    {
        "create", "read", "update", "delete", "echo"
    };

    public Response ValidateRequest(Request request)
    {
        var errors = new List<string>();

        // method
        if (string.IsNullOrEmpty(request.Method))
            errors.Add("missing method");
        else if (!ValidMethods.Contains(request.Method))
            errors.Add("illegal method");

        // path
        if (string.IsNullOrEmpty(request.Path))
            errors.Add("missing path");

        // date
        if (request.Date is null)
        {
            errors.Add("missing date");
        }
        else
        {
            switch (request.Date)
            {
                case long:
                case int:
                case short:
                case byte:
                    break;
                case string s:
                    if (!long.TryParse(s, out _))
                        errors.Add("illegal date");
                    break;
                case JsonElement je:
                    if (je.ValueKind == JsonValueKind.Number)
                    {
                        if (!je.TryGetInt64(out _))
                            errors.Add("illegal date");
                    }
                    else if (je.ValueKind == JsonValueKind.String)
                    {
                        var sv = je.GetString();
                        if (string.IsNullOrEmpty(sv) || !long.TryParse(sv, out _))
                            errors.Add("illegal date");
                    }
                    else
                    {
                        errors.Add("illegal date");
                    }
                    break;
                default:
                    errors.Add("illegal date");
                    break;
            }
        }

        // body
        if (request.Method is "create" or "update" or "echo")
        {
            if (request.Body is null)
            {
                errors.Add("missing body");
            }
            else if (request.Method is "create" or "update")
            {
                try
                {
                    if (request.Body is string s)
                        JsonDocument.Parse(s);
                    else
                        JsonDocument.Parse(JsonSerializer.Serialize(request.Body));
                }
                catch
                {
                    errors.Add("illegal body");
                }
            }
        }

        if (errors.Count > 0)
        {
            return new Response { Status = "4 " + string.Join(", ", errors) };
        }

        return new Response { Status = "1 Ok" };
    }
}