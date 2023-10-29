using AzureStorageLibrary.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary.Repositories.Interfaces
{

    public interface IAzureStorage
    {
        /// <summary>
        /// This method uploads a file submitted with the request
        /// </summary>
        /// <param name="file">File for upload</param>
        /// <returns>Blob with status</returns>
        Task<BlobResponseDto> UploadAsync(IFormFile file, string orgCode, string fileType = null, string contentFolder = null);

        Task<BlobResponseDto>  CreateBlob(byte[] data, string orgCode, string fileType = null, string contentFolder = null, string fileExt = null);
        Task<BlobDto> DownloadAsync(string blobFilename);
        
    }
}
