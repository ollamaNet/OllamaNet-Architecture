using ConversationService.Infrastructure.Document.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ConversationService.Infrastructure.Document.Storage;

/// <summary>
/// File system based implementation of document storage
/// </summary>
public class FileSystemDocumentStorage : IDocumentStorage
{
    private readonly DocumentManagementOptions _options;
    private readonly string _basePath;

    public FileSystemDocumentStorage(IOptions<DocumentManagementOptions> options)
    {
        _options = options.Value;
        _basePath = Path.GetFullPath(_options.StoragePath);
        
        // Ensure storage directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    /// <inheritdoc/>
    public async Task<string> SaveAsync(Stream fileStream, string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        if (string.IsNullOrWhiteSpace(fileName)) 
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        string uniquePath = GenerateSecureFilePath(fileName);
        string fullPath = Path.Combine(_basePath, uniquePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using var fileStream2 = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fileStream2);

        return uniquePath;
    }

    /// <inheritdoc/>
    public Task<Stream> GetAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        string fullPath = GetSecureFullPath(filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", filePath);

        return Task.FromResult<Stream>(
            new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        string fullPath = GetSecureFullPath(filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        string fullPath = GetSecureFullPath(filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private string GenerateSecureFilePath(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        
        using var sha256 = SHA256.Create();
        var hashInput = $"{fileName}_{timestamp}_{Guid.NewGuid()}";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var extension = Path.GetExtension(fileName);
        
        return Path.Combine(datePath, hashString.Substring(0, 2), hashString.Substring(2, 2), $"{hashString}{extension}");
    }

    private string GetSecureFullPath(string filePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(_basePath, filePath));

        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access to the path is denied");
        }

        return fullPath;
    }
} 