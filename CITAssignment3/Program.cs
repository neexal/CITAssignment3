using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CITAssignment3;

public class Program
{
    static void Main()
    {
        TcpListener server = new(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Server running on port 5000...");
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            new Thread(() => HandleClient(client)).Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        string requestJson = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(requestJson))
            return;

        Request req;
        try { req = JsonSerializer.Deserialize<Request>(requestJson); }
        catch
        {
            writer.Write("{\"status\":\"4 Bad Request\"}");
            return;
        }

        var validator = new RequestValidator();
        var validation = validator.ValidateRequest(req);
        writer.Write(JsonSerializer.Serialize(validation));
    }
}