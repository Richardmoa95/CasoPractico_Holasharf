using IntegrationServices.Domain.Common;

namespace IntegrationServices.Domain.ValueObjects;

public sealed record Evidence
{
    public string Label { get; }
    public string FileType { get; }
    public string FileName { get; }
    public string SourceUrl { get; }
    public string? StoredUrl { get; private set; }

    private Evidence(string label, string fileType, string fileName, string sourceUrl, string? storedUrl = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainException("Evidence label is required.");
        }

        if (string.IsNullOrWhiteSpace(fileType))
        {
            throw new DomainException("Evidence file type is required.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("Evidence file name is required.");
        }

        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            throw new DomainException("Evidence source URL is required.");
        }

        Label = label;
        FileType = fileType;
        FileName = fileName;
        SourceUrl = sourceUrl;
        StoredUrl = storedUrl;
    }

    public static Evidence Create(string label, string fileType, string fileName, string sourceUrl)
    {
        return new Evidence(label, fileType, fileName, sourceUrl);
    }

    public Evidence MarkAsStored(string storedUrl)
    {
        if (string.IsNullOrWhiteSpace(storedUrl))
        {
            throw new DomainException("Stored URL is required.");
        }

        return this with { StoredUrl = storedUrl };
    }
}