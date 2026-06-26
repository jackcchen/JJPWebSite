using System.Globalization;
using Azure.Identity;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CsvReader.Api;

public sealed class CsvItemsFunction
{
    private readonly ILogger<CsvItemsFunction> _logger;

    public CsvItemsFunction(ILogger<CsvItemsFunction> logger)
    {
        _logger = logger;
    }

    [Function("csv-items")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "csv-items")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var storageAccountName = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME");
        var containerName = Environment.GetEnvironmentVariable("CSV_CONTAINER_NAME");
        var blobName = Environment.GetEnvironmentVariable("CSV_BLOB_NAME");

        if (string.IsNullOrWhiteSpace(storageAccountName) ||
            string.IsNullOrWhiteSpace(containerName) ||
            string.IsNullOrWhiteSpace(blobName))
        {
            return new ObjectResult(new { message = "Storage configuration is missing." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        try
        {
            var blobUri = new Uri(
                $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}");

            var blobClient = new BlobClient(blobUri, new DefaultAzureCredential());
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
                TrimOptions = TrimOptions.Trim
            };

            await using var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, csvConfiguration);

            var records = csv.GetRecords<CsvItem>().ToList();

            return new OkObjectResult(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read CSV blob {Container}/{Blob}.", containerName, blobName);

            return new ObjectResult(new { message = "CSV data could not be loaded from blob storage." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
