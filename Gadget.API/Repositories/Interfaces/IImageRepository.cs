using Gadget.API.APIModel;
using Gadget.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IImageRepository : IRepository<Image>
    {
        Task<List<APIImage>> GetImageList();
        Task<List<Image>> PostImageList(Image imageData);
        Task<List<APIImage>> GetImageById(int id);
        Task<int> SaveImage(IFormFile uploadedFile, Image image);
        bool DeleteFile(string filePath);
        Task<List<Image>> GetImageByIdForDelete(int id);
    }
}
