using System.Text.Json.Serialization;

namespace StarterApp.Database.Models;

// represents a single item.
// contains the getter/setter methods for the item.
public class Item
{
    // basic data for the item.
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // daily amount that the item is loaned out for.
    public decimal DailyRate { get; set; }

    // sets a category for the item.
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // sets an owner for the item.
    // important for being able to update item as only
    // owner should be updating.
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    // for setting item location.
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // for checking if able to be rented (bool=true default).
    public bool IsAvailable { get; set; } = true;

    // sets item creation/modifcation times.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}