﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Linq;

namespace CITAssignment3;

public class Program
{
    private static readonly CategoryService Service = new();
    static void Main()
    {
        TcpListener server = new(IPAddress.Any, 5000);
        try
        {
            // Help avoid TIME_WAIT reuse issues
            server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.Start();
        }
        catch (SocketException ex) when ((int)ex.SocketErrorCode == 10048)
        {
            Console.Error.WriteLine("Port 5000 is already in use. Stop the other process and try again.");
            return;
        }
        Console.WriteLine("Server running on port 5000...");
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            new Thread(() => HandleClient(client)).Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string requestJson;
            try
            {
                requestJson = reader.ReadToEnd();
            }
            catch (IOException)
            {
                // Client disconnected abruptly; ignore
                return;
            }

            if (string.IsNullOrWhiteSpace(requestJson))
                return;

            // Use case-insensitive property names for incoming JSON (supports lowercase keys like "method")
            var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Request req;
            try
            {
                req = JsonSerializer.Deserialize<Request>(requestJson, deserializeOptions);
            }
            catch
            {
                writer.Write("{\"status\":\"4 Bad Request\"}");
                return;
            }
            if (req == null)
            {
                writer.Write("{\"status\":\"4 Bad Request\"}");
                return;
            }

            var validator = new RequestValidator();
            var validation = validator.ValidateRequest(req);
            if (validation.Status != "1 Ok")
            {
                var respOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                writer.Write(JsonSerializer.Serialize(validation, respOptions));
                return;
            }

            // Process the request
            var parser = new UrlParser();
            if (!string.Equals(req.Method, "echo", StringComparison.OrdinalIgnoreCase))
            {
                if (!parser.ParseUrl(req.Path))
                {
                    writer.Write("{\"status\":\"4 Bad Request\"}");
                    return;
                }
            }

            Response resp;
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            switch (req.Method)
        {
            case "read":
                if (parser.HasId)
                {
                    var cat = Service.GetCategory(parser.Id);
                    if (cat == null)
                        resp = new Response { Status = "5 Not found" };
                    else
                        resp = new Response { Status = "1 Ok", Body = cat };
                }
                else
                {
                    var cats = Service.GetCategories();
                    resp = new Response { Status = "1 Ok", Body = cats };
                }
                break;
            case "create":
                if (parser.HasId)
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                Category? createCat = null;
                try
                {
                    createCat = req.Body is string sCreate
                        ? JsonSerializer.Deserialize<Category>(sCreate, options)
                        : JsonSerializer.Deserialize<Category>(JsonSerializer.Serialize(req.Body), options);
                }
                catch (JsonException)
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                if (createCat == null || string.IsNullOrWhiteSpace(createCat.Name))
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                int newId = Service.GetCategories().Any() ? Service.GetCategories().Max(c => c.Id) + 1 : 1;
                Service.CreateCategory(newId, createCat.Name);
                var newCat = Service.GetCategory(newId);
                resp = new Response { Status = "2 Created", Body = newCat };
                break;
            case "update":
                if (!parser.HasId)
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                Category? updateCat = null;
                try
                {
                    updateCat = req.Body is string sUpdate
                        ? JsonSerializer.Deserialize<Category>(sUpdate, options)
                        : JsonSerializer.Deserialize<Category>(JsonSerializer.Serialize(req.Body), options);
                }
                catch (JsonException)
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                if (updateCat != null && !string.IsNullOrWhiteSpace(updateCat.Name) && Service.UpdateCategory(parser.Id, updateCat.Name))
                    resp = new Response { Status = "3 Updated" };
                else
                    resp = new Response { Status = "5 Not found" };
                break;
            case "delete":
                if (!parser.HasId)
                {
                    resp = new Response { Status = "4 Bad Request" };
                    break;
                }
                if (Service.DeleteCategory(parser.Id))
                    resp = new Response { Status = "1 Ok" };
                else
                    resp = new Response { Status = "5 Not found" };
                break;
            case "echo":
                // Body must be plain text; if JSON provided, echo its raw string
                var echoed = req.Body is string sEcho ? sEcho : JsonSerializer.Serialize(req.Body);
                resp = new Response { Status = "1 Ok", Body = echoed };
                break;
            default:
                resp = new Response { Status = "4 Bad Request" };
                break;
        }

            writer.Write(JsonSerializer.Serialize(resp, options));
        }
        catch (IOException)
        {
            // Network I/O issue from client; ignore
        }
        catch (SocketException)
        {
            // Client socket error; ignore
        }
    }
}
