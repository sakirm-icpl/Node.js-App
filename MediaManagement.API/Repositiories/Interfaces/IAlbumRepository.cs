// ======================================
// <copyright file="IAlbumRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using MediaManagement.API.APIModel;
using MediaManagement.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MediaManagement.API.Repositories.Interfaces
{
    public interface IAlbumRepository : IRepository<MediaLibraryAlbum>
    {
        Task<int> GetIdIfExist(string category);
        Task<int> GetLastInsertedId();
        Task<IEnumerable<MediaLibraryAlbum>> Search(string category);
        Task<IEnumerable<APIMediaLibraryAlbum>> GetAlbum(int userid);
    }
}
