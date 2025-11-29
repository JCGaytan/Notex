using Notex.Core.Models;

namespace Notex.Core.Services;

public class NoteService : INoteService
{
    private readonly IAirtableService _airtableService;

    public NoteService(IAirtableService airtableService)
    {
        _airtableService = airtableService;
    }

    public Task<List<Note>> GetNotesAsync(string? bookId = null, CancellationToken cancellationToken = default)
    {
        return _airtableService.GetNotesAsync(bookId, cancellationToken);
    }

    public Task<Note> CreateNoteAsync(string bookId, string title, CancellationToken cancellationToken = default)
    {
        var note = new Note
        {
            BookId = bookId,
            Title = title,
            Content = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return _airtableService.CreateNoteAsync(note, cancellationToken);
    }

    public Task<Note> UpdateNoteAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.UpdatedAt = DateTime.UtcNow;
        return _airtableService.UpdateNoteAsync(note, cancellationToken);
    }

    public Task DeleteNoteAsync(Note note, CancellationToken cancellationToken = default)
    {
        return _airtableService.DeleteNoteAsync(note.Id, cancellationToken);
    }
}
