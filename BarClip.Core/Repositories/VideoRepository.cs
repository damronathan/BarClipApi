using BarClipApi.Data;
using BarClipApi.Data.Schema;
using Microsoft.EntityFrameworkCore;
using BarClipApi.Core.Services;
using BarClipApi.Models.Requests;

namespace BarClipApi.Core.Repositories;

public class VideoRepository
{
    private readonly AppDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly SessionRepository _sessionRepository;

    public VideoRepository(AppDbContext context, StorageService storageService, UserRepository userRepository, SessionRepository sessionRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }
    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task AddVideoAsync(Video video)
    {
        // Clear navigation properties to avoid tracking issues
        video.User = null;
        video.Session = null;

        // Verify foreign keys are set
        if (video.UserId == null)
            throw new InvalidOperationException("Video.UserId cannot be null");

        if (video.SessionId == Guid.Empty)
            throw new InvalidOperationException("Video.SessionId cannot be empty");

        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOriginalVideoAsync(Video video)
    {
        // Fetch the existing entity and update only specific properties
        var existing = await _context.Videos.FindAsync(video.Id);

        if (existing == null)
            throw new InvalidOperationException($"Video {video.Id} not found");

        // **Placeholder, add update logic here.

        await _context.SaveChangesAsync();
    }
    public async Task<List<Video>> GetAllVideosForSessionAsync(Guid? sessionId)
    {
        return await _context.Videos
            .Where(v => v.SessionId == sessionId)
                    .AsNoTracking()
            .ToListAsync();
    }
    public async Task<List<Video>> GetAllVideosForUserAsync(Guid? userId)
    {
        return await _context.Videos
            .Where(v => v.UserId == userId)
                    .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Video> CreateVideoAsync(Video video)
    {
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
        return video;
    }
    public async Task<Video?> GetVideoByIdAsync(Guid id)
    {
        return await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task SaveVideoAsync(VideoRequest request)
    {
        // Ensure related entities are known to EF

        var user = await _userRepository.GetOrCreateUserAsync(request.UserId);
        await _sessionRepository.GetOrCreateSessionAsync(request.SessionId, user.Id);

        // Find or create the original video (and track it)
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId);

        if (video == null)
        {
            video = new Video
            {
                Id = request.VideoId,
                UserId = user.Id,
                SessionId = request.SessionId,
                CreatedAt = request.CreatedAt,
                OrderNumber = request.OrderNumber,
                IsFull = request.IsFull
            };
            _context.Videos.Add(video);
        }
        else
        {
            video.Id = request.VideoId;
            video.SessionId = request.SessionId;
            video.UserId = user.Id;
            video.CreatedAt = request.CreatedAt;
        }

        await _context.SaveChangesAsync();
    }

}
