// ======================================
// <copyright file="AlbumRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.APIModel;
using Publication.API.Data;
using Publication.API.Helper;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace Publication.API.Repositories
{
    public class AlbumRepository : Repository<MediaLibraryAlbum>, IAlbumRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AlbumRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public AlbumRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this._customerConnectionString = customerConnectionString;
            this.db = context;
        }

        public async Task<int> GetIdIfExist(string category)
        {
            IQueryable<int> result = (from album in this.db.MediaLibraryAlbum
                                      where ((album.AlbumName.ToLower() == category.ToLower() && album.IsDeleted == Record.NotDeleted))
                                      select album.Id);
            if (result != null)
                return await result.FirstOrDefaultAsync();

            return 0;
        }

        public async Task<int> GetLastInsertedId()
        {
            IQueryable<int> result = (from album in this.db.MediaLibraryAlbum
                                      orderby album.Id descending
                                      select album.Id);
            if (result != null)
                //return await result.ToAsyncEnumerable().First();
                return await result.FirstOrDefaultAsync();
            return 0;
        }
        public async Task<IEnumerable<MediaLibraryAlbum>> Search(string Category)
        {
            try
            {
                List<MediaLibraryAlbum> result = await (from album in this.db.MediaLibraryAlbum
                                                        where (album.AlbumName.StartsWith(Category) && album.IsDeleted == Record.NotDeleted)
                                                        select new MediaLibraryAlbum
                                                        {
                                                            AlbumName = album.AlbumName,
                                                            Id = album.Id,
                                                            ShowToAll = album.ShowToAll

                                                        }).AsNoTracking().ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<APIMediaLibraryAlbum>> GetAlbum(int userid)
        {
            try
            {
                APIMediaLibraryAlbum album = new APIMediaLibraryAlbum();
                List<APIMediaLibraryAlbum> albums = new List<APIMediaLibraryAlbum>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAlbum";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userid });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                album = new APIMediaLibraryAlbum
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    AlbumName = row["AlbumName"].ToString(),
                                };
                                albums.Add(album);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return albums;
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
