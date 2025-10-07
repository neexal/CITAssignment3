﻿namespace CITAssignment3;

using System.Text.Json.Serialization;

public class Category
{
	// Map to "cid" when serialized over the wire while keeping Id for tests/internal use
	[JsonPropertyName("cid")]
	public int Id { get; set; }
	public string Name { get; set; }
}
