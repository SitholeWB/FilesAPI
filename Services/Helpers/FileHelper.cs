namespace Services;

public sealed class FileHelper : IDisposable
{
    private readonly string _filePath;

    public FileHelper(Stream stream, string name)
    {
        _filePath = Path.Combine(Path.GetTempPath(), "FilesAPI");
        if (!Directory.Exists(_filePath))
        {
            Directory.CreateDirectory(_filePath);
        }
        _filePath = Path.Combine(Path.GetTempPath(), "FilesAPI", $"{Guid.NewGuid()}_{name}");
        using var fileStream = File.Create(_filePath);
        stream.CopyTo(fileStream);
    }

    public string GetFilePath() => _filePath;

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
        GC.SuppressFinalize(this);
    }
}