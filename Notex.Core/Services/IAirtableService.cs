using Notex.Core.Models;

namespace Notex.Core.Services;

public interface IAirtableService
{
    Task<bool> ConfigureAsync(string apiToken, string? baseId = null, CancellationToken cancellationToken = default);
    string? ApiToken { get; }
    string? BaseId { get; }
    Task EnsureBaseAndTablesAsync(CancellationToken cancellationToken = default);

    Task<List<Book>> GetBooksAsync(CancellationToken cancellationToken = default);
    Task<Book> CreateBookAsync(Book book, CancellationToken cancellationToken = default);
    Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default);
    Task DeleteBookAsync(string bookId, CancellationToken cancellationToken = default);

    Task<List<Note>> GetNotesAsync(string? bookId = null, CancellationToken cancellationToken = default);
    Task<Note> CreateNoteAsync(Note note, CancellationToken cancellationToken = default);
    Task<Note> UpdateNoteAsync(Note note, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(string noteId, CancellationToken cancellationToken = default);
}
