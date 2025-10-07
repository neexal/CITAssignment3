using System.Collections.Generic;
using System.Linq;

namespace CITAssignment3;

public class CategoryService
{
    private readonly List<Category> categories = new()
    {
        new Category { Id = 1, Name = "Beverages" },
        new Category { Id = 2, Name = "Condiments" },
        new Category { Id = 3, Name = "Confections" }
    };

    public List<Category> GetCategories()
    {
        return categories;
    }

    public Category? GetCategory(int id)
    {
        return categories.FirstOrDefault(c => c.Id == id);
    }

    public bool CreateCategory(int id, string name)
    {
        if (categories.Any(c => c.Id == id)) return false;
        categories.Add(new Category { Id = id, Name = name });
        return true;
    }

    public bool UpdateCategory(int id, string newName)
    {
        var category = categories.FirstOrDefault(c => c.Id == id);
        if (category == null) return false;
        category.Name = newName;
        return true;
    }

    public bool DeleteCategory(int id)
    {
        var category = categories.FirstOrDefault(c => c.Id == id);
        if (category == null) return false;
        categories.Remove(category);
        return true;
    }
}