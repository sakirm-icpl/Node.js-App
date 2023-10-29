using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary.Repositories
{
    #region Dependency Injection / Constructor
    public class AzureStorage : IAzureStorage
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        // private readonly ILogger<AzureStorage> _logger;
        public AzureStorage(IConfiguration configuration)
        {
            _storageConnectionString = configuration["BlobConnectionString"];
            _storageContainerName = configuration["BlobContainerName"];
            // _logger = logger;
        }
        #endregion

        public async Task<List<BlobDto>> ListAsync()
        {
            // Get a reference to a container named in appsettings.json
            BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName);

            // Create a new list object for 
            List<BlobDto> files = new List<BlobDto>();

            await foreach (BlobItem file in container.GetBlobsAsync())
            {
                // Add each file retrieved from the storage container to the files list by creating a BlobDto object
                string uri = container.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}/{name}";

                files.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType
                });
            }

            // Return all files to the requesting method
            return files;
        }

        
        public async Task<BlobResponseDto> UploadAsync(IFormFile blob, string orgCode, string fileType = null, string contentFolder = null)
        {
            // Create new upload response object that we can return to the requesting method
            BlobResponseDto response = new BlobResponseDto();

            var filename = GenerateFileName(blob.FileName, orgCode, fileType,contentFolder);

            var options = new BlobClientOptions {
                Retry = {
        Delay = TimeSpan.FromSeconds(2),
        MaxRetries = 10,
        Mode = RetryMode.Exponential,
        MaxDelay = TimeSpan.FromSeconds(10),
        NetworkTimeout = TimeSpan.FromSeconds(300)
         },
                Transport = new HttpClientTransport(
                     new HttpClient { Timeout = TimeSpan.FromSeconds(102) } )
            };
            options.Diagnostics.IsLoggingEnabled = false;
            options.Diagnostics.IsTelemetryEnabled = false;
            options.Diagnostics.IsDistributedTracingEnabled = false;
            options.Retry.MaxRetries = 0;
            // Get a reference to a container named in appsettings.json and then create it
            BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName,options:options);
            //await container.CreateAsync();
            try
            {
                
                // Get a reference to the blob just uploaded from the API in a container from configuration settings
                BlobClient client = container.GetBlobClient(filename);

                // Open a stream for the file we want to upload
                await using (Stream? data = blob.OpenReadStream())
                {
                    // Upload the file async
                    await client.UploadAsync(data);
                }

                // Everything is OK and file got uploaded
                response.Status = $"File {blob.FileName} Uploaded Successfully";
                response.Error = false;
                response.Blob.Uri = client.Uri.AbsoluteUri;
                response.Blob.Name = client.Name;

            }
            // If the file already exists, we catch the exception and do not upload it
            catch (RequestFailedException ex)
               when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
              //  _logger.LogError($"File with name {blob.FileName} already exists in container. Set another name to store the file in the container: '{_storageContainerName}.'");
                response.Status = $"File with name {blob.FileName} already exists. Please use another name to store your file.";
                response.Error = true;
                return response;
            }
            // If we get an unexpected error, we catch it here and return the error message
            catch (RequestFailedException ex)
            {
                // Log error to console and create a new response we can return to the requesting method
                //_logger.LogError($"Unhandled Exception. ID: {ex.StackTrace} - Message: {ex.Message}");
                response.Status = $"Unexpected error: {ex.StackTrace}. Check log with StackTrace ID.";
                response.Error = true;
                return response;
            }

            // Return the BlobUploadResponse object
            return response;
        }

        private string GenerateFileName(string fileName, string ordCode, string fileType = null,string contentFolder=null)
        {
            try
            {
                string strFileName = string.Empty;
                string[] strName = fileName.Split('.');

                if (!string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(contentFolder))
                {
                    fileName = Path.Combine(ordCode, contentFolder, fileType, DateTime.Now.Ticks + strName[0].Trim() + "." + strName[strName.Length - 1]);
                }
               else if (!string.IsNullOrEmpty(fileType) && string.IsNullOrEmpty(contentFolder))
                {
                    fileName = Path.Combine(ordCode, fileType, DateTime.Now.Ticks + strName[0].Trim() + "." + strName[strName.Length - 1]);
                }
                else if (string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(contentFolder))
                {
                    fileName = Path.Combine(ordCode, contentFolder, DateTime.Now.Ticks + strName[0].Trim() + "." + strName[strName.Length - 1]);
                }
                else
                {
                    fileName = Path.Combine(ordCode, DateTime.Now.Ticks + strName[0].Trim() + "." + strName[strName.Length - 1]);
                }
                fileName = string.Concat(fileName.Split(' '));


                strFileName = Path.Combine(fileName);
                return strFileName;
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }

        public async Task<BlobResponseDto> CreateBlob(byte[] data, string orgCode, string fileType = null,string contentFolder=null,string fileExt = null)
        {
            BlobResponseDto response = new BlobResponseDto();
            string file = ".png";
            if (!String.IsNullOrEmpty(fileExt))
                file = fileExt;
              var filename = GenerateFileName(file, orgCode, fileType, contentFolder);

            
            try
            {
                CloudStorageAccount storageAccount;
                CloudBlobClient client;
                CloudBlobContainer container;
                CloudBlockBlob blob;

                storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                client = storageAccount.CreateCloudBlobClient();
               container =  client.GetContainerReference(_storageContainerName);
               
                await container.CreateIfNotExistsAsync();
                blob = container.GetBlockBlobReference(filename);              
                await blob.UploadFromByteArrayAsync(data, 0, data.Length);

                response.Status = $"File {response.Blob.Name} Uploaded Successfully";
                response.Error = false;
                response.Blob.Uri = blob.Uri.AbsoluteUri;
                response.Blob.Name = blob.Name;

            }
            // If the file already exists, we catch the exception and do not upload it
            catch (RequestFailedException ex)
               when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                //  _logger.LogError($"File with name {blob.FileName} already exists in container. Set another name to store the file in the container: '{_storageContainerName}.'");
                response.Status = $"File with name {file} already exists. Please use another name to store your file.";
                response.Error = true;
                return response;
            }
            // If we get an unexpected error, we catch it here and return the error message
            catch (RequestFailedException ex)
            {
                // Log error to console and create a new response we can return to the requesting method
                //_logger.LogError($"Unhandled Exception. ID: {ex.StackTrace} - Message: {ex.Message}");
                response.Status = $"Unexpected error: {ex.StackTrace}. Check log with StackTrace ID.";
                response.Error = true;
                return response;
            }

            // Return the BlobUploadResponse object
            return response;

        }

        public async Task<BlobDto> DownloadAsync(string blobFilename)
        {
            // Get a reference to a container named in appsettings.json
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);

            try
            {
                // Get a reference to the blob uploaded earlier from the API in the container from configuration settings
                BlobClient file = client.GetBlobClient(blobFilename);

                // Check if the file exists in the container
                if (await file.ExistsAsync())
                {
                    var data = await file.OpenReadAsync();
                    Stream blobContent = data;

                    // Download the file details async
                    var content = await file.DownloadContentAsync();

                    // Add data to variables in order to return a BlobDto
                    string name = blobFilename;
                    string contentType = content.Value.Details.ContentType;

                    // Create new BlobDto with blob data from variables
                    return new BlobDto { Content = blobContent, Name = name, ContentType = contentType, Uri=file.Uri.ToString() };
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // Log error to console
               // _logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist, return null and handle that in requesting method
            return null;
        }
    }

}

