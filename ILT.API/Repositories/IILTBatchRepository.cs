using ILT.API.APIModel;
//using ILT.API.APIModel.ILT;
using ILT.API.Common;
using ILT.API.Model.ILT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IILTBatchRepository : IRepository<ILTBatch>
    {
        Task<BatchCode> GetBatchCode(int UserId);
        Task CancelBatchCode(APIBatchCode aPIBatchCode, int UserId);
        Task<ApiResponse> PostBatch(APIILTBatch aPIILTBatch, int UserId);
        Task<ApiResponse> PutBatch(APIILTBatch aPIILTBatch, int UserId);
        Task<APIILTBatch> GetBatch(int Id);
        Task<List<APIILTBatchDetails>> GetBatches(int UserId, int page, int pageSize, string search = null, string searchText = null, bool? IsExport = null);
        Task<int> GetBatchCount(int UserId, string search = null, string searchText = null);
        Task<byte[]> ExportImportFormat(string OrgCode);
        Task<ApiResponse> ProcessImportFile(APIILTBatchImport aPIILTBatchImport, int UserId, string OrgCode);
        Task<List<APIILTBatchRejected>> GetBatchRejected(int page, int pageSize);
        Task<FileInfo> ExportILTBatchReject();
        Task<int> CountRejected();
        Task<MessageType> DeleteBatch(APIILTBatchDelete aPIILTBatchDelete);
        Task<string> IsBatchwiseNominationEnabled();
        Task<ApiResponse> GetBatchName(int CourseId, string search = null);
        Task<FileInfo> ExportBatches(int UserId, string OrgCode, string search = null, string searchText = null);
        Task<ApiResponse> GetBatchForNomination(int CourseId, string search = null);
    }
}
