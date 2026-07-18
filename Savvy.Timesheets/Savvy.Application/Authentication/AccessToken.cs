namespace Savvy.Application.Authentication;

public sealed record AccessToken(string Value, DateTime ExpiresUtc);
