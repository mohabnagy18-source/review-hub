using Microsoft.EntityFrameworkCore;
using ReviewHub.Application.Abstractions;
using ReviewHub.Application.DTOs;
using ReviewHub.Domain.Entities;

namespace ReviewHub.Application.Services;

public class PlaceService(IApplicationDbContext db)
{
    public async Task<List<PlaceDto>> GetPlacesAsync(string? kind, string? search, CancellationToken ct = default)
    {
        var q = db.Places.Include(p => p.Reviews).AsQueryable();
        if (!string.IsNullOrWhiteSpace(kind) && Enum.TryParse<PlaceKind>(kind, true, out var k))
            q = q.Where(p => p.Kind == k);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(s) || p.Category.ToLower().Contains(s));
        }
        var places = await q.ToListAsync(ct);
        return places
            .OrderByDescending(p => p.Reviews.Count == 0 ? 0 : p.Reviews.Average(r => r.Rating))
            .ThenByDescending(p => p.Reviews.Count)
            .Select(Map)
            .ToList();
    }

    public async Task<PlaceDetailDto?> GetPlaceAsync(Guid id, CancellationToken ct = default)
    {
        var p = await db.Places
            .Include(x => x.Reviews).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return null;
        var reviews = p.Reviews.OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.PlaceId, p.Name, r.User?.Name ?? "Anonymous", r.Rating, r.Text, r.CreatedAt))
            .ToList();
        return new PlaceDetailDto(p.Id, p.Kind.ToString(), p.Name, p.Category, p.Description, p.Emoji, p.Meta,
            reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1), reviews);
    }

    public async Task<List<ReviewDto>> GetMyReviewsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Reviews.Include(r => r.Place).Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.PlaceId, r.Place!.Name, r.User!.Name, r.Rating, r.Text, r.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ReviewDto> UpsertReviewAsync(Guid userId, Guid placeId, CreateReviewRequest req, CancellationToken ct = default)
    {
        if (req.Rating < 1 || req.Rating > 5) throw new ArgumentException("Rating must be between 1 and 5.");
        var place = await db.Places.FirstOrDefaultAsync(p => p.Id == placeId, ct)
            ?? throw new KeyNotFoundException("Place not found.");
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);

        var review = await db.Reviews.FirstOrDefaultAsync(r => r.PlaceId == placeId && r.UserId == userId, ct);
        if (review is null)
        {
            review = new Review { PlaceId = placeId, UserId = userId, Rating = req.Rating, Text = req.Text.Trim() };
            db.Reviews.Add(review);
        }
        else
        {
            review.Rating = req.Rating;
            review.Text = req.Text.Trim();
            review.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return new ReviewDto(review.Id, placeId, place.Name, user.Name, review.Rating, review.Text, review.CreatedAt);
    }

    private static PlaceDto Map(Place p) => new(
        p.Id, p.Kind.ToString(), p.Name, p.Category, p.Description, p.Emoji, p.Meta,
        p.Reviews.Count == 0 ? 0 : Math.Round(p.Reviews.Average(r => r.Rating), 1),
        p.Reviews.Count);
}
