using Microsoft.EntityFrameworkCore;
using ReviewHub.Domain.Entities;

namespace ReviewHub.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Places.AnyAsync()) return;

        var places = new List<Place>
        {
            new() { Kind = PlaceKind.Restaurant, Name = "Osteria Bella", Category = "Italian", Emoji = "🍝", Meta = "Downtown", Description = "Cozy trattoria known for handmade pasta and a slow-simmered ragù." },
            new() { Kind = PlaceKind.Restaurant, Name = "Sakura House", Category = "Japanese", Emoji = "🍣", Meta = "Riverside", Description = "Minimalist sushi bar with a daily omakase and fresh nigiri." },
            new() { Kind = PlaceKind.Restaurant, Name = "El Camino", Category = "Mexican", Emoji = "🌮", Meta = "Old Town", Description = "Street-style tacos, house margaritas and a lively patio." },
            new() { Kind = PlaceKind.Restaurant, Name = "The Golden Fork", Category = "Fine Dining", Emoji = "🥂", Meta = "City Center", Description = "Seasonal tasting menus and an award-winning wine list." },
            new() { Kind = PlaceKind.Restaurant, Name = "Smoke & Barrel", Category = "BBQ", Emoji = "🍖", Meta = "Warehouse District", Description = "Twelve-hour smoked brisket, ribs and craft beer on tap." },
            new() { Kind = PlaceKind.Restaurant, Name = "Verde", Category = "Vegan", Emoji = "🥗", Meta = "Garden Quarter", Description = "Plant-based bowls, cold-pressed juices and weekend brunch." },
            new() { Kind = PlaceKind.Movie, Name = "The Last Horizon", Category = "Sci-Fi", Emoji = "🚀", Meta = "2025", Description = "A stranded crew races a dying star to deliver the map home." },
            new() { Kind = PlaceKind.Movie, Name = "Whispers in the Fog", Category = "Thriller", Emoji = "🌫️", Meta = "2024", Description = "A detective returns to her hometown where the fog keeps secrets." },
            new() { Kind = PlaceKind.Movie, Name = "Paper Lanterns", Category = "Drama", Emoji = "🏮", Meta = "2025", Description = "Two estranged sisters inherit their grandmother's lantern shop." },
            new() { Kind = PlaceKind.Movie, Name = "Midnight Circuit", Category = "Action", Emoji = "🏎️", Meta = "2026", Description = "An underground racer takes one last job across three cities." },
            new() { Kind = PlaceKind.Movie, Name = "The Baker of Lyon", Category = "Comedy", Emoji = "🥖", Meta = "2024", Description = "A failed magician becomes the accidental star baker of Lyon." },
            new() { Kind = PlaceKind.Movie, Name = "Echoes of Tomorrow", Category = "Animation", Emoji = "🎐", Meta = "2025", Description = "A girl who hears the future tries to change one small day." }
        };

        db.Places.AddRange(places);
        await db.SaveChangesAsync();

        var demo = new User { Name = "Demo Reviewer", Email = "demo@reviewhub.app", PasswordHash = "100000.demo.demo" };
        db.Users.Add(demo);
        await db.SaveChangesAsync();

        var rnd = new Random(42);
        var samples = new (int Rating, string Text)[]
        {
            (5, "Absolutely loved it. Would come back in a heartbeat."),
            (4, "Really good overall, just a bit of a wait at peak hours."),
            (5, "A standout. Every detail felt considered."),
            (3, "Decent, though the hype set my expectations too high."),
            (4, "Great atmosphere and friendly staff. Recommended."),
            (2, "Not for me. The pacing felt off and it dragged in the middle."),
            (5, "Instant classic. I keep telling everyone about it."),
            (4, "Solid experience, a couple of small misses but worth it.")
        };

        foreach (var p in places)
        {
            var count = rnd.Next(2, 5);
            for (var i = 0; i < count; i++)
            {
                var s = samples[rnd.Next(samples.Length)];
                db.Reviews.Add(new Review
                {
                    PlaceId = p.Id,
                    UserId = demo.Id,
                    Rating = s.Rating,
                    Text = s.Text,
                    CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 60))
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
