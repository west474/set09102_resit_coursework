namespace StarterApp.Database.Models;

// represents a group of similar items.
// contains the getters/setters for categories.
public class Category
{ 
    public int Id { get; set; }
 
    // human readable category name.
    public string Name { get; set; } = string.Empty;

    // url-friendly category name for navigation.
    public string Slug { get; set; } = string.Empty;
}