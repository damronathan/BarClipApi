using BarClipApi.Data.Schema;
using BarClipApi.Data;
using Microsoft.EntityFrameworkCore;
using BarClipApi.Models.Requests;

namespace BarClipApi.Core.Repositories;
public class SessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateSessionAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }
    public async Task<Session> UpdateSessionAsync(UpdateSessionRequest request)
    {
        var session = await GetSessionByIdAsync(request.Id);
        if (session == null)
        {
            throw new InvalidOperationException("No session found");
        }
        session.Title = request.Title;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }
    public async Task<Session> GetOrCreateSessionAsync(Guid sessionId, Guid userId)
    {
        var existingSession = await GetSessionByIdAsync(sessionId);
        if (existingSession is not null)
        {
            return existingSession;
        }
        else
        {
            var newSession = new Session
            {
                Id = sessionId,
                UserId = userId
            };
            _context.Sessions.Add(newSession);
            await _context.SaveChangesAsync();
            return newSession;
        }
    }
    public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
    {
        var result = await _context.Sessions.FindAsync(sessionId);
        return result;
    }
    public async Task<List<Session>> GetSessionsByUserIdAsync(Guid userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId)
            .Select(s => new Session
            {
                Id = s.Id,
                Title = s.Title,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var session = await GetSessionByIdAsync(sessionId);
        if (session != null)
        {
            _context.Sessions.Remove(session);
        }
        else
        {
            throw new InvalidOperationException("No session found");
        }
            await _context.SaveChangesAsync();
    }


}
