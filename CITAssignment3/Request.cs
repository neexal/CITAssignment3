namespace CITAssignment3;

public class Request
{
    public string Method { get; set; }
    public string Path { get; set; }
    // Accept both numeric and string assignment in tests and JSON
    public object Date { get; set; }
    // Accept either JSON object (for create/update) or string (for echo)
    public object? Body { get; set; }
}
