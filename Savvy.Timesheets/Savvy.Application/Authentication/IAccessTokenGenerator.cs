using Savvy.Domain;

namespace Savvy.Application.Authentication;

public interface IAccessTokenGenerator
{
    AccessToken Generate(User user);
}
