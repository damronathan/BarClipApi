using BarClip.Core.Repositories;
using BarClip.Data.Schema;
using BarClip.Models.Dto;
using BarClip.Models.Requests;
using System.Windows.Markup;

namespace BarClip.Core.Services;
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
    public async Task<Session> CreateSessionWithFolders(User user, string basePath, string title)
    {
        var session = await CreateSession(user, title);

        var sessionPath = Path.Combine(basePath, session.Id.ToString());
        var folders = new[]
        {
        sessionPath,
        Path.Combine(sessionPath, "Thumbnails"),
        Path.Combine(sessionPath, "Processed"),
        Path.Combine(sessionPath, "Original")
    };

        foreach (var folder in folders)
        {
            Directory.CreateDirectory(folder);
        }

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
        var session = await _repo.UpdateSessionAsync(Id, request);
        
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
