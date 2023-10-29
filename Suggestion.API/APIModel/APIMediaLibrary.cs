// ======================================
// <copyright file="APIMediaLibrary.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.Models;
using Suggestion.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.APIModel
{
    public class APIMediaLibrary //: CommonFields
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        
        public string Album { get; set; }
        [MaxLength(500)]
        [Required]
        public string FileType { get; set; }
        public int AlbumId { get; set; }
        public bool ShowToAll { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(100)]
        [Required]
        public string ObjectTitle { get; set; }
        [MaxLength(2000)]
        public string FilePath { get; set; }
        [MaxLength(20)]
        [CSVInjection]
        public string Keywords { get; set; }
        public int LikesCount { get; set; }
        [MaxLength(1000)]
        public string Metadata { get; set; }

        public MediaLibrary MapAPIToMediaLibrary(APIMediaLibrary aPIMediaLibrary, int token)
        {
            MediaLibrary mediaLibrary = new MediaLibrary
            {
                Id = aPIMediaLibrary.Id,
                Date = aPIMediaLibrary.Date,
                Album = aPIMediaLibrary.Album,
                FileType = aPIMediaLibrary.FileType,
                AlbumId = aPIMediaLibrary.AlbumId,
                ObjectId = aPIMediaLibrary.ObjectId,
                ObjectTitle = aPIMediaLibrary.ObjectTitle,
                FilePath = aPIMediaLibrary.FilePath,
                Keywords = aPIMediaLibrary.Keywords,
                LikesCount = aPIMediaLibrary.LikesCount,
                CreatedBy = token,
                Metadata = aPIMediaLibrary.Metadata,
            };
            return mediaLibrary;
        }

        public APIMediaLibrary MapAPIMediaLibraryToAPI(MediaLibrary mediaLibrary)
        {
            APIMediaLibrary aPIMediaLibrary = new APIMediaLibrary
            {
                Id = mediaLibrary.Id,
                Date = mediaLibrary.Date,
                Album = mediaLibrary.Album,
                FileType = mediaLibrary.FileType,
                AlbumId = mediaLibrary.AlbumId,
                ObjectId = mediaLibrary.ObjectId,
                ObjectTitle = mediaLibrary.ObjectTitle,
                FilePath = mediaLibrary.FilePath,
                Keywords = mediaLibrary.Keywords,
                LikesCount = mediaLibrary.LikesCount,
                Metadata = mediaLibrary.Metadata,
            };
            //aPIMediaLibrary.CreatedBy = mediaLibrary.CreatedBy;
            return aPIMediaLibrary;
        }
    }
    public class APIMediaLibraryAlbum //: CommonFields
    {
        public int Id { get; set; }
        public string AlbumName { get; set; }
        public bool ShowToAll { get; set; }
    }


    public class APIMediaLibraryBulk //: CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        
        public string Album { get; set; }
        public int AlbumId { get; set; }
        public bool ShowToAll { get; set; }
        public int LikesCount { get; set; }
        [MaxLength(1000)]
        public string Metadata { get; set; }
        public APIMediaLibraryMerge[] aPIMediaLibraryMerge { get; set; }
        public MediaLibrary MapAPIToMediaLibrary(APIMediaLibraryBulk aPIMediaLibrary, int token)
        {
            MediaLibrary mediaLibrary = new MediaLibrary
            {
                Id = aPIMediaLibrary.Id,
                Date = aPIMediaLibrary.Date,
                Album = aPIMediaLibrary.Album,
                //mediaLibrary.FileType = aPIMediaLibrary.FileType;
                AlbumId = aPIMediaLibrary.AlbumId,
                //mediaLibrary.ObjectId = aPIMediaLibrary.ObjectId;
                //mediaLibrary.ObjectTitle = aPIMediaLibrary.ObjectTitle;
                //mediaLibrary.FilePath = aPIMediaLibrary.FilePath;
                //mediaLibrary.Keywords = aPIMediaLibrary.Keywords;
                LikesCount = aPIMediaLibrary.LikesCount
            };
             mediaLibrary.CreatedBy = token;
            return mediaLibrary;
        }

        public APIMediaLibrary MapAPIMediaLibraryToAPI(APIMediaLibraryBulk mediaLibrary)
        {
            APIMediaLibrary aPIMediaLibrary = new APIMediaLibrary
            {
                Id = mediaLibrary.Id,
                Date = mediaLibrary.Date,
                Album = mediaLibrary.Album,
                //aPIMediaLibrary.FileType = mediaLibrary.FileType;
                AlbumId = mediaLibrary.AlbumId,
                //aPIMediaLibrary.ObjectId = mediaLibrary.ObjectId;
                //aPIMediaLibrary.ObjectTitle = mediaLibrary.ObjectTitle;
                //aPIMediaLibrary.FilePath = mediaLibrary.FilePath;
                //aPIMediaLibrary.Keywords = mediaLibrary.Keywords;
                LikesCount = mediaLibrary.LikesCount
            };
            //aPIMediaLibrary.CreatedBy = mediaLibrary.CreatedBy;
            return aPIMediaLibrary;
        }
    }


    public class APIMediaLibraryMerge
    {
        public string FileType { get; set; }
        public int? ObjectId { get; set; }
        [MaxLength(100)]
        [Required]
        [CSVInjection]
        public string ObjectTitle { get; set; }
        [MaxLength(2000)]
        public string FilePath { get; set; }
        [MaxLength(20)]
        [CSVInjection]
        public string Keywords { get; set; }
    }
}
