using BarClipApi.Core.Repositories;
using BarClipApi.Data.Schema;
using BarClipApi.Models.Dto;
using BarClipApi.Models.Requests;
using System.Windows.Markup;

namespace BarClipApi.Core.Services;
public class SessionService
{
    private readonly SessionRepository _repo;
    private readonly StorageService _storageService;
    private readonly UserRepository _userRepository;
    public SessionService(SessionRepository repo, StorageService storageService, UserRepository userRepository)
    {
        _repo = repo;
        _storageService = storageService;
        _userRepository = userRepository;
    }
    public async Task<Session> CreateSession(User? user, string title)
    {
        var session = new Session
        {
            User = user,
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.CreateSessionAsync(session);
        return session;
    }
    public async Task<List<SessionDto>> GetAllSessions(string userId)
    {
        var user = await _userRepository.GetOrCreateUserAsync(userId);
        var sessions = await _repo.GetSessionsByUserIdAsync(user.Id);
        var sessionDtos = new List<SessionDto>();
        foreach (var session in sessions)
        {
            var thumbnailUrl = _storageService.GenerateDownloadSasUrl(new SasUrlRequest { Id = session.Id, ContainerName = "sessions", Extension = ".jpg" });
            var sessionDto = new SessionDto
            {
                Id = session.Id,
                CreatedDate = session.CreatedAt,
                Title = session.Title,
                ThumbnailUrl = thumbnailUrl
            };
            sessionDtos.Add(sessionDto);
        }
        return sessionDtos;
    }
    public async Task DeleteSession(Guid sessionId)
    {
        await _repo.DeleteSessionAsync(sessionId);
    }
    public async Task<SessionDto> UpdateSession(Guid Id, UpdateSessionRequest request)
    {
        var session = await _repo.UpdateSessionAsync(request);
        
        var thumbnailUrl = _storageService.GenerateDownloadSasUrl(new SasUrlRequest { Id = session.Id, ContainerName = "sessions", Extension = ".jpg" });
        var sessionDto = new SessionDto
        {
            Id = session.Id,
            CreatedDate = session.CreatedAt,
            Title = session.Title,
            ThumbnailUrl = thumbnailUrl
        };
        return sessionDto;
    }
}
