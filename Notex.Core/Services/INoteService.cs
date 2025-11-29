using Notex.Core.Models;

namespace Notex.Core.Services;

public interface INoteService
{
    Task<List<Note>> GetNotesAsync(string? bookId = null, CancellationToken cancellationToken = default);
    Task<Note> CreateNoteAsync(string bookId, string title, CancellationToken cancellationToken = default);
    Task<Note> UpdateNoteAsync(Note note, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(Note note, CancellationToken cancellationToken = default);
}
