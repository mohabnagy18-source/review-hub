namespace ReviewHub.Application.DTOs;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Name, string Email);

public record PlaceDto(Guid Id, string Kind, string Name, string Category, string Description,
    string Emoji, string Meta, double AverageRating, int ReviewCount);

public record ReviewDto(Guid Id, Guid PlaceId, string PlaceName, string AuthorName, int Rating,
    string Text, DateTime CreatedAt);

public record PlaceDetailDto(Guid Id, string Kind, string Name, string Category, string Description,
    string Emoji, string Meta, double AverageRating, List<ReviewDto> Reviews);

public record CreateReviewRequest(int Rating, string Text);
