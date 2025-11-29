using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Notex.Core.Models;
using Notex.Core.Services;
using Notex.UI.Views;

namespace Notex.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAirtableService _airtableService;
    private IBookService? _bookService;
    private INoteService? _noteService;

    private Book? _selectedBook;
    private Note? _selectedNote;
    private string _connectionStatus = "Disconnected";
    private Brush _connectionStatusColor = Brushes.OrangeRed;
    private bool _isBusy;

    public MainViewModel(ISettingsService settingsService, IAirtableService airtableService, IBookService? bookService, INoteService? noteService)
    {
        _settingsService = settingsService;
        _airtableService = airtableService;
        _bookService = bookService;
        _noteService = noteService;

        Books = new ObservableCollection<Book>();
        Notes = new ObservableCollection<Note>();

        CreateBookCommand = new RelayCommand(async _ => await CreateBookAsync(), _ => _bookService is not null && !_isBusy);
        RenameBookCommand = new RelayCommand(async _ => await RenameBookAsync(), _ => SelectedBook is not null && !_isBusy);
        DeleteBookCommand = new RelayCommand(async _ => await DeleteBookAsync(), _ => SelectedBook is not null && !_isBusy);
        CreateNoteCommand = new RelayCommand(async _ => await CreateNoteAsync(), _ => SelectedBook is not null && !_isBusy);
        DeleteNoteCommand = new RelayCommand(async _ => await DeleteNoteAsync(), _ => SelectedNote is not null && !_isBusy);
        SaveNoteCommand = new RelayCommand(async _ => await SaveNoteAsync(), _ => SelectedNote is not null && !_isBusy);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

        _ = InitializeAsync();
    }

    public ObservableCollection<Book> Books { get; }
    public ObservableCollection<Note> Notes { get; }

    public Book? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (SetProperty(ref _selectedBook, value))
            {
                _ = LoadNotesAsync();
                ((RelayCommand)CreateNoteCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public Note? SelectedNote
    {
        get => _selectedNote;
        set
        {
            if (SetProperty(ref _selectedNote, value))
            {
                ((RelayCommand)DeleteNoteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveNoteCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set => SetProperty(ref _connectionStatus, value);
    }

    public Brush ConnectionStatusColor
    {
        get => _connectionStatusColor;
        private set => SetProperty(ref _connectionStatusColor, value);
    }

    public ICommand CreateBookCommand { get; }
    public ICommand RenameBookCommand { get; }
    public ICommand DeleteBookCommand { get; }
    public ICommand CreateNoteCommand { get; }
    public ICommand DeleteNoteCommand { get; }
    public ICommand SaveNoteCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    private async Task InitializeAsync()
    {
        await RefreshConnectionAsync();
        if (_bookService is not null)
        {
            await LoadBooksAsync();
        }
    }

    private async Task RefreshConnectionAsync()
    {
        if (_bookService is null || _noteService is null)
        {
            ConnectionStatus = "Disconnected";
            ConnectionStatusColor = Brushes.OrangeRed;
            return;
        }

        ConnectionStatus = "Connected";
        ConnectionStatusColor = Brushes.LightGreen;
    }

    private async Task LoadBooksAsync()
    {
        if (_bookService is null)
        {
            return;
        }

        _isBusy = true;
        try
        {
            Books.Clear();
            var books = await _bookService.GetBooksAsync();
            foreach (var book in books.OrderByDescending(b => b.UpdatedAt))
            {
                Books.Add(book);
            }
            SelectedBook = Books.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load books: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task LoadNotesAsync()
    {
        if (_noteService is null || SelectedBook is null)
        {
            Notes.Clear();
            return;
        }

        _isBusy = true;
        try
        {
            Notes.Clear();
            var notes = await _noteService.GetNotesAsync(SelectedBook.Id);
            foreach (var note in notes.OrderByDescending(n => n.UpdatedAt))
            {
                Notes.Add(note);
            }
            SelectedNote = Notes.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load notes: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task CreateBookAsync()
    {
        if (_bookService is null)
        {
            MessageBox.Show("Please configure Airtable first.", "Notex", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = "New Book";
        _isBusy = true;
        try
        {
            var created = await _bookService.CreateBookAsync(name);
            Books.Insert(0, created);
            SelectedBook = created;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create book: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task RenameBookAsync()
    {
        if (_bookService is null || SelectedBook is null)
        {
            return;
        }

        var newName = Microsoft.VisualBasic.Interaction.InputBox("Rename book", "Notex", SelectedBook.Name);
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        _isBusy = true;
        try
        {
            var updated = await _bookService.RenameBookAsync(SelectedBook, newName);
            var index = Books.IndexOf(SelectedBook);
            Books[index] = updated;
            SelectedBook = updated;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to rename book: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task DeleteBookAsync()
    {
        if (_bookService is null || SelectedBook is null)
        {
            return;
        }

        if (MessageBox.Show($"Delete '{SelectedBook.Name}'?", "Notex", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        _isBusy = true;
        try
        {
            await _bookService.DeleteBookAsync(SelectedBook);
            Books.Remove(SelectedBook);
            SelectedBook = Books.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete book: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task CreateNoteAsync()
    {
        if (_noteService is null || SelectedBook is null)
        {
            MessageBox.Show("Please select a book first.", "Notex", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _isBusy = true;
        try
        {
            var note = await _noteService.CreateNoteAsync(SelectedBook.Id, "Untitled Note");
            Notes.Insert(0, note);
            SelectedNote = note;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create note: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task DeleteNoteAsync()
    {
        if (_noteService is null || SelectedNote is null)
        {
            return;
        }

        if (MessageBox.Show($"Delete note '{SelectedNote.Title}'?", "Notex", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        _isBusy = true;
        try
        {
            await _noteService.DeleteNoteAsync(SelectedNote);
            Notes.Remove(SelectedNote);
            SelectedNote = Notes.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete note: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task SaveNoteAsync()
    {
        if (_noteService is null || SelectedNote is null)
        {
            return;
        }

        _isBusy = true;
        try
        {
            var updated = await _noteService.UpdateNoteAsync(SelectedNote);
            var index = Notes.IndexOf(SelectedNote);
            Notes[index] = updated;
            SelectedNote = updated;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save note: {ex.Message}", "Notex", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_settingsService, _airtableService);
        var window = new SettingsWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            _bookService = new BookService(_airtableService);
            _noteService = new NoteService(_airtableService);
            _ = RefreshConnectionAsync();
            _ = LoadBooksAsync();
        }
    }
}
