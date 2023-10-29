using Courses.API.APIModel;
using Courses.API.APIModel.ThirdPartyIntegration;
using Courses.API.Common;
using Courses.API.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ILCMSRepository : IRepository<LCMS>
    {
        Task<bool> Exists(string name);
        Task<List<LCMS>> Get(int page, int pageSize, string search = null, string contentType = null);
        Task<int> Count(string search = null, string contentType = null);
        Task<Object> MediaCount(int userId,bool showAllData = false);
        Task<IEnumerable<object>> Media(int page, int pageSize, string search = null, string metaData = null);
        Task<IEnumerable<APILCMSMedia>> SurveyMedia(int page, int pageSize, string search = null, string metaData = null);
        Task<int> GetMediaCount(string search = null, string metaData = null);
        Task<int> GetSurveyMediaCount(string search = null, string metaData = null);
        Task<bool> Exist(string FileName, string contentType, int? id = null);
        Task<ApiResponse> SaveH5P(IFormFile uploadedFile, LCMSAPI lcmsApi, int UserId, string ordCode);
        Task<ApiResponse> SaveZip(IFormFile uploadedFile, LCMSAPI lcmsApi, int UserId, string ordCode);
        Task<ApiResponse> SaveVideo(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode);
        Task<ApiResponse> SaveAudio(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode);
        Task<ApiResponse> SavePdf(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode);
        Task<ApiResponse> SaveImage(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode);
        Task<bool> SaveFile(IFormFile uploadedFile, string filePath);
        bool GetThumbnail(string imagePath, string thumbnailPath);
        Task<int> AddYoutubeFile(LCMSAPI lcmsApi, int UserId);
        Task<int> AddAssesment(LCMSAPI lcmsApi, int UserId);
        Task<int> AddFeedback(LCMSAPI lcmsApi, int UserId);
        Task<int> AddSurvey(LCMSAPI lcmsApi, int UserId);
        Task<LCMS> GetLcmsByAssessmentConfigureId(int AssesmentConfigId);
        Task<Message> DeleteLcms(int lcmsId);
        Task<bool> FileExist(string fileName, string version);
        Task<bool> ExistByType(string name, string contentType = null, int? id = null);
        Task<bool> ExistYouTubeLink(string name, string contentType = null, string YoutubeVideoId = null, int? id = null);
        Task<bool> ExistByTypeExternalLink(string name, string contentType = null, string Path = null, int? id = null);
        Task<IEnumerable<object>> GetxAPILaunchData(int moduleid, string orgnizationCode, int UserId);
        Task<APITotalLCMsView> MediaV2(ApiGetLCMSMedia apiGetLCMSMedia, int userId, string userRole);
        Task<int> SaveVimeoLink(VimeoVideo vimeoVideo, int UserId);
        Task<VimeoConfiguration> GetVimeoToken();
        Task<VimeoLink> GetVimeoVideo(LCMSID lCMSID);
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
        Task<int> GetAuthenticationKpoint(IFormFile formFile, string fileName, string name, string Description, LCMS lCMS,int UserId);
        string KPointToken(int UserId);
        string KPointTokenForAdmin();
        Task<List<string>> GetExternalLinkVendor();
        Task<List<string>> GetVideoSubContent();
    }

}
