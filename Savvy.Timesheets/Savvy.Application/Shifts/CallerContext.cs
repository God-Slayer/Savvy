using Savvy.Domain;

namespace Savvy.Application.Shifts;

public sealed record CallerContext(Guid UserId, UserRole Role, Guid? PracticeId);
