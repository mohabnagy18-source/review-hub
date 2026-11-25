namespace ReviewHub.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public enum PlaceKind
{
    Restaurant = 0,
    Movie = 1
}

public class Place
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PlaceKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Emoji { get; set; } = "⭐";
    public string Meta { get; set; } = string.Empty;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlaceId { get; set; }
    public Place? Place { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
