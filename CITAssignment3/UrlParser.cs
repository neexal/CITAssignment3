﻿namespace CITAssignment3;

public class UrlParser
{
    public string Path { get; set; }
    public int Id { get; set; }
    public bool HasId { get; set; }

    public bool ParseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        var parts = url.Trim('/').Split('/');
        if (parts.Length < 2)
            return false;

        Path = "/" + parts[0] + "/" + parts[1];

        if (parts.Length > 2)
        {
            if (int.TryParse(parts[2], out int id))
            {
                Id = id;
                HasId = true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}
