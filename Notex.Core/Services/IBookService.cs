using Notex.Core.Models;

namespace Notex.Core.Services;

public interface IBookService
{
    Task<List<Book>> GetBooksAsync(CancellationToken cancellationToken = default);
    Task<Book> CreateBookAsync(string name, CancellationToken cancellationToken = default);
    Task<Book> RenameBookAsync(Book book, string newName, CancellationToken cancellationToken = default);
    Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default);
}
