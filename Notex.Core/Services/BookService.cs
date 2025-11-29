using Notex.Core.Models;

namespace Notex.Core.Services;

public class BookService : IBookService
{
    private readonly IAirtableService _airtableService;

    public BookService(IAirtableService airtableService)
    {
        _airtableService = airtableService;
    }

    public Task<List<Book>> GetBooksAsync(CancellationToken cancellationToken = default)
    {
        return _airtableService.GetBooksAsync(cancellationToken);
    }

    public Task<Book> CreateBookAsync(string name, CancellationToken cancellationToken = default)
    {
        var book = new Book
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return _airtableService.CreateBookAsync(book, cancellationToken);
    }

    public async Task<Book> RenameBookAsync(Book book, string newName, CancellationToken cancellationToken = default)
    {
        book.Name = newName;
        return await _airtableService.UpdateBookAsync(book, cancellationToken);
    }

    public Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        return _airtableService.DeleteBookAsync(book.Id, cancellationToken);
    }
}
