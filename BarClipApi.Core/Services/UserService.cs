using BarClipApi.Core.Repositories;
using BarClipApi.Data.Schema;
using BarClipApi.Models.Requests;

namespace BarClipApi.Core.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task WakeUp()
    {
        await _userRepository.GetByNameIdentifierAsync("");
    }
    public async Task<User> GetOrCreateUserAsync(string nameIdentifier, string? email = null)
    {
        var existingUser = await _userRepository.GetByNameIdentifierAsync(nameIdentifier);

        if (existingUser != null)
        {
            if ((email != null && existingUser.Email != email))
            {
                existingUser.Email = email ?? existingUser.Email;
                await _userRepository.UpdateAsync(existingUser);
            }
            return existingUser;
        }

        var newUser = new User
        {
            Email = email,          
        };

        await _userRepository.CreateAsync(newUser);
        return newUser;
    }
    public async Task UpdateUser(string entraId, UpdateUserRequest request)
    {
        var user = await GetOrCreateUserAsync(entraId);

        user.Email = request.Email;
        await _userRepository.UpdateUserAsync(user);
    }

    public async Task DeleteUser(string entraId)
    {
        var user = await GetOrCreateUserAsync(entraId);

        await _userRepository.DeleteUserAsync(user);
    }
}
