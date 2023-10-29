// ======================================
// <copyright file="MediaLibraryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using MediaManagement.API.APIModel;
using MediaManagement.API.Data;
using MediaManagement.API.Helper;
using MediaManagement.API.Models;
using MediaManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace MediaManagement.API.Repositories
{
    public class MediaLibraryRepository : Repository<MediaLibrary>, IMediaLibraryRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MediaLibraryRepository));
        private GadgetDbContext db;
        private IAlbumRepository _albumRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public MediaLibraryRepository(GadgetDbContext context,
            IAlbumRepository albumRepository,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            this._albumRepository = albumRepository;
        }
        //public async Task<List<APIMediaLibrary>> GetAllMediaLibrary(int UserId, string UserRole, int page, int pageSize, string search = null)
        //{
        //    try
        //    {

        //IQueryable<APIMediaLibrary> Query = (from mediaLibrary in db.MediaLibrary
        //                                     join mediaLibraryAlbum in db.MediaLibraryAlbum on mediaLibrary.AlbumId equals mediaLibraryAlbum.Id
        //                                     where mediaLibrary.IsDeleted == false
        //                                     select new APIMediaLibrary
        //                                     {
        //                                         Album = mediaLibrary.Album,
        //                                         ObjectTitle = mediaLibrary.ObjectTitle,
        //                                         ObjectId = mediaLibrary.ObjectId,
        //                                         AlbumId = mediaLibrary.AlbumId,
        //                                         FilePath = mediaLibrary.FilePath,
        //                                         Keywords = mediaLibrary.Keywords,
        //                                         Id = mediaLibrary.Id,
        //                                         FileType = mediaLibrary.FileType,
        //                                         LikesCount = mediaLibrary.LikesCount,
        //                                         ShowToAll = mediaLibraryAlbum.ShowToAll,
        //                                         Date = mediaLibrary.Date
        //                                     });

        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            Query = Query.Where(r => r.Album.Contains(search));
        //        }
        //        Query = Query.Where(v => v.CreatedBy == UserId);
        //        Query = Query.OrderByDescending(r => r.Id);
        //        if (page != -1)
        //            Query = Query.Skip((page - 1) * pageSize);
        //        if (pageSize != -1)
        //            Query = Query.Take(pageSize);

        //        List<APIMediaLibrary> aPIMediaLibrary = await Query.ToListAsync();

        //        return aPIMediaLibrary;



        //    }
        //    catch (Exception ex)
        //    {
        //        string exception = ex.Message;
        //    }
        //    return null;
        //}

        public async Task<List<APIMediaLibrary>> GetAllMediaLibrary(int UserId, string UserRole, int page, int pageSize, string search = null)
        {

            try
            {
                IQueryable<APIMediaLibrary> Query = (from mediaLibrary in db.MediaLibrary
                                                     join mediaLibraryAlbum in db.MediaLibraryAlbum on mediaLibrary.AlbumId equals mediaLibraryAlbum.Id
                                                     where mediaLibrary.IsDeleted == false
                                                     select new APIMediaLibrary
                                                     {
                                                         Album = mediaLibrary.Album,
                                                         ObjectTitle = mediaLibrary.ObjectTitle,
                                                         ObjectId = mediaLibrary.ObjectId,
                                                         AlbumId = mediaLibrary.AlbumId,
                                                         FilePath = mediaLibrary.FilePath,
                                                         Keywords = mediaLibrary.Keywords,
                                                         Id = mediaLibrary.Id,
                                                         FileType = mediaLibrary.FileType,
                                                         LikesCount = mediaLibrary.LikesCount,
                                                         ShowToAll = mediaLibraryAlbum.ShowToAll,
                                                         Date = mediaLibrary.Date,
                                                         Metadata= mediaLibrary.Metadata
                                                     });

                if (!string.IsNullOrEmpty(search))
                {

                    Query = Query.Where(v => v.Album.Contains(search) || v.ObjectTitle.Contains(search));
                    Query = Query.OrderByDescending(v => v.Id);
                }
                
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }



        public async Task<int> Count(int UserId, string UserRole, string search = null)
        {

            IQueryable<APIMediaLibrary> Query = (from mediaLibrary in db.MediaLibrary
                                                 join mediaLibraryAlbum in db.MediaLibraryAlbum on mediaLibrary.AlbumId equals mediaLibraryAlbum.Id
                                                 where mediaLibrary.IsDeleted == false
                                                 select new APIMediaLibrary
                                                 {
                                                     Album = mediaLibrary.Album,
                                                     ObjectTitle = mediaLibrary.ObjectTitle,
                                                     ObjectId = mediaLibrary.ObjectId,
                                                     AlbumId = mediaLibrary.AlbumId,
                                                     FilePath = mediaLibrary.FilePath,
                                                     Keywords = mediaLibrary.Keywords,
                                                     Id = mediaLibrary.Id,
                                                     FileType = mediaLibrary.FileType,
                                                     LikesCount = mediaLibrary.LikesCount,
                                                     ShowToAll = mediaLibraryAlbum.ShowToAll,
                                                     Date = mediaLibrary.Date
                                                 });

            if (!string.IsNullOrEmpty(search))
            {

                Query = Query.Where(v => v.Album.Contains(search) || v.ObjectTitle.Contains(search));
                Query = Query.OrderByDescending(v => v.Id);
            }

            Query = Query.OrderByDescending(v => v.Id);
           
            return await Query.CountAsync();

        }
        public async Task<bool> Exist(string search)
        {
            int count = await this.db.MediaLibrary.Where(p => p.Album.ToLower() == search.ToLower()).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistTitle(int albumid, string search)
        {
            if (albumid != 0)
            {
                int count = await this.db.MediaLibrary.Where(p => (p.ObjectTitle.ToLower() == search.ToLower() && p.AlbumId == albumid && p.IsDeleted == Record.NotDeleted)).CountAsync();

                if (count > 0)
                    return true;
                return false;
            }
            return false;
        }
        public async Task<bool> ExistObjectTitle(string ObjectTitle)
        {
            int count = await this.db.MediaLibrary.Where(c => (c.ObjectTitle.ToLower() == ObjectTitle.ToLower() && c.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<int> GetTotalMediaLibraryCount()
        {
            return await this.db.MediaLibrary.CountAsync();
        }
        public async Task<IEnumerable<MediaLibrary>> Search(string query)
        {
            Task<List<MediaLibrary>> mediaLibraryList = (from mediaLibrary in this.db.MediaLibrary
                                                         where
                                                         (mediaLibrary.Album.StartsWith(query) ||
                                                        Convert.ToString(mediaLibrary.ObjectTitle).StartsWith(query)
                                                        )
                                                         && mediaLibrary.IsDeleted == false
                                                         select mediaLibrary).ToListAsync();
            return await mediaLibraryList;
        }
        public async Task<IEnumerable<APIMediaLibraryAlbum>> SearchAlbum(int userid, string album)
        {
            try
            {
                APIMediaLibraryAlbum mediaLibraryAlbum = new APIMediaLibraryAlbum();
                List<APIMediaLibraryAlbum> mediaLibraryAlbums = new List<APIMediaLibraryAlbum>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetMediaLibraryAlbum";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userid });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = album });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                mediaLibraryAlbum = new APIMediaLibraryAlbum
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    AlbumName = row["AlbumName"].ToString(),
                                };
                                mediaLibraryAlbums.Add(mediaLibraryAlbum);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return mediaLibraryAlbums;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<MediaLibrary> GetMediaLibraryObject(MediaLibrary mediaLibrary, APIMediaLibrary aPIMediaLibrary)
        {
            int albumId = await this._albumRepository.GetIdIfExist(aPIMediaLibrary.Album);
            if (albumId != 0)
                mediaLibrary.AlbumId = albumId;
            else if (!string.IsNullOrEmpty(aPIMediaLibrary.Album))
            {
                MediaLibraryAlbum album = new MediaLibraryAlbum
                {
                    AlbumName = aPIMediaLibrary.Album,
                    ShowToAll = aPIMediaLibrary.ShowToAll,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = mediaLibrary.CreatedBy,
                    ModifiedBy = mediaLibrary.CreatedBy
                };
                await this._albumRepository.Add(album);
                int aid = await this._albumRepository.GetLastInsertedId();
                mediaLibrary.AlbumId = aid;
            }
            return mediaLibrary;
        }


        public async Task<MediaLibrary> GetMediaLibraryObjectMerge(MediaLibrary mediaLibrary, APIMediaLibraryBulk aPIMediaLibrary)
        {
            int albumId = await this._albumRepository.GetIdIfExist(aPIMediaLibrary.Album);
            if (albumId != 0)
                mediaLibrary.AlbumId = albumId;
            else if (!string.IsNullOrEmpty(aPIMediaLibrary.Album))
            {
                MediaLibraryAlbum album = new MediaLibraryAlbum
                {
                    AlbumName = aPIMediaLibrary.Album,
                    ShowToAll = aPIMediaLibrary.ShowToAll,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = mediaLibrary.CreatedBy,
                    ModifiedBy = mediaLibrary.CreatedBy
                };
                await this._albumRepository.Add(album);
                int aid = await this._albumRepository.GetLastInsertedId();
                mediaLibrary.AlbumId = aid;
            }
            return mediaLibrary;
        }
        public async Task<IEnumerable<MediaLibrary>> GetAllMediaLibraryByAlbumId(int id)
        {
            try
            {
                IQueryable<Models.MediaLibrary> Query = this.db.MediaLibrary;
                Query = Query.Where(v => v.AlbumId == id && (v.IsDeleted == Record.NotDeleted));
                Query = Query.OrderByDescending(v => v.Id);
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }


        public async Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId)
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "RewardPointsDaily_Upsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = functionCode });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category });
                        cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                        cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = point });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<APIMediaLibrary> GetTopOneMedia()
        {
            try
            {
                IQueryable<APIMediaLibrary> result = (from mediaLibrary in this.db.MediaLibrary
                                                      where (mediaLibrary.IsDeleted == Record.NotDeleted
                                                      )
                                                      select new APIMediaLibrary
                                                      {
                                                          Album = mediaLibrary.Album,
                                                          FileType = mediaLibrary.FileType,
                                                          Id = mediaLibrary.Id,
                                                          AlbumId = mediaLibrary.AlbumId,
                                                          Date = mediaLibrary.Date,
                                                          ObjectId = mediaLibrary.ObjectId,
                                                          ObjectTitle = mediaLibrary.ObjectTitle,
                                                          FilePath = mediaLibrary.FilePath
                                                      });
                return await result.FirstAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

       
    }
}
