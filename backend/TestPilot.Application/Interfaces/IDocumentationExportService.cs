namespace TestPilot.Application.Interfaces;

public interface IDocumentationExportService
{
    Task<string> ExportCollectionAsMarkdownAsync(Guid collectionId, Guid userId);
    Task<string> ExportCollectionAsPostmanJsonAsync(Guid collectionId, Guid userId);
}
