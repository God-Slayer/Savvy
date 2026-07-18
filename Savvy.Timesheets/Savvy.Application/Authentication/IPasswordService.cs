using Savvy.Domain;

namespace Savvy.Application.Authentication;

public interface IPasswordService
{
    bool Verify(User user, string password);
}
