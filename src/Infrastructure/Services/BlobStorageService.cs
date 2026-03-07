using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace SecurityReport.Infrastructure.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(string container, string fileName, Stream content, string contentType);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _client;

        public BlobStorageService(IConfiguration config)
        {
            var conn = config["BLOB_CONNECTION"];
            if (string.IsNullOrWhiteSpace(conn)) throw new ArgumentNullException("BLOB_CONNECTION");
            _client = new BlobServiceClient(conn);
        }

        public async Task<string> UploadAsync(string container, string fileName, Stream content, string contentType)
        {
            var containerClient = _client.GetBlobContainerClient(container);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }
    }
}