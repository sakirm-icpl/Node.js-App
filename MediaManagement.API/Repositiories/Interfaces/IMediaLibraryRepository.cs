// ======================================
// <copyright file="IMediaLibraryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using MediaManagement.API.APIModel;
using MediaManagement.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaManagement.API.Repositories.Interfaces
{
    public interface IMediaLibraryRepository : IRepository<MediaLibrary>
    {
        Task<List<APIMediaLibrary>> GetAllMediaLibrary(int UserId, string UserRole, int page, int pageSize, string search = null);
        Task<int> Count(int UserId, string UserRole, string search = null);
        Task<bool> Exist(string search);
        Task<bool> ExistTitle(int albumid, string search);
        Task<bool> ExistObjectTitle(string ObjectTitle);
        Task<int> GetTotalMediaLibraryCount();
        Task<IEnumerable<MediaLibrary>> Search(string query);
        Task<IEnumerable<APIMediaLibraryAlbum>> SearchAlbum(int userid, string album);
        Task<MediaLibrary> GetMediaLibraryObject(MediaLibrary mediaLibrary, APIMediaLibrary aPIMediaLibrary);
        Task<MediaLibrary> GetMediaLibraryObjectMerge(MediaLibrary mediaLibrary, APIMediaLibraryBulk aPIMediaLibrary);
        Task<IEnumerable<MediaLibrary>> GetAllMediaLibraryByAlbumId(int id);
        Task<APIMediaLibrary> GetTopOneMedia();
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
       
    }
}
