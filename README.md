# Notex

Notex is a simple Evernote-like Windows desktop client built with WPF and .NET 8.0. It stores books (notebooks) and notes remotely in Airtable using async HTTP APIs while keeping the UI responsive through MVVM.

## Projects
- `Notex.Core` — domain models, settings management, Airtable integration, and book/note services.
- `Notex.UI` — WPF application with MVVM view models and XAML views.

## Configuration
1. Launch the app. If no Airtable token is saved yet, open **Settings** from the top bar.
2. Paste your **Airtable API token** and (optionally) a **Base Id**. Leaving Base Id blank lets Notex try to discover or reuse a base.
3. Save. The app configures the Airtable client and attempts to prepare required tables:
   - `Books` with fields `Id`, `Name`, `CreatedAt`, `UpdatedAt`.
   - `Notes` with fields `Id`, `BookId`, `Title`, `Content`, `CreatedAt`, `UpdatedAt`.

## Airtable base creation limitations
Airtable's public API cannot create bases or tables programmatically. Notex attempts to discover an existing base named **Notex** via the metadata API. If it cannot find or access a base, create one manually in the Airtable UI and supply its Base Id in Settings.

## Building and running
- Requires the .NET 8.0 SDK (or newer) on Windows.
- Build from the solution root:
  ```bash
  dotnet build Notex.sln
  dotnet run --project Notex.UI/Notex.UI.csproj
  ```

## Usage
- Books appear in the left pane with controls to add, rename, or delete.
- Selecting a book shows its notes in the middle pane; you can create or delete notes there.
- The right pane edits the selected note's title and content. Use **Save** to persist changes to Airtable.
- Connection status to Airtable is displayed in the header.
