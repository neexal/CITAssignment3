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
        if (string.IsNullOrEmpty(request.Date))
        {
            errors.Add("missing date");
        }
        else if (!long.TryParse(request.Date, out _))
        {
            errors.Add("illegal date");
        }

        // body
        if (request.Method is "create" or "update" or "echo")
        {
            if (string.IsNullOrEmpty(request.Body))
            {
                errors.Add("missing body");
            }
            else if (request.Method is "create" or "update")
            {
                try
                {
                    JsonDocument.Parse(request.Body);
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