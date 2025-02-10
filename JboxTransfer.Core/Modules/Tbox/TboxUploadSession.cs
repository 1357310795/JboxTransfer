using JboxTransfer.Core.Extensions;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Tbox;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Extensions;
using Teru.Code.Models;

namespace JboxTransfer.Core.Modules.Tbox
{
    /// <summary>
    /// 用于管理上传整个文件的会话。支持由confirmKey进行断点续传
    /// </summary>
    public class TboxUploadSession
    {
        private readonly TboxService _tbox;
        public TboxUploadSession(TboxService tbox)
        {
            _tbox = tbox;
        }

        private string path;
        private long size;
        private int chunkCount;
        private string confirmKey;
        public string ConfirmKey => confirmKey;
        private List<TboxUploadPartSession> remainParts;
        public List<TboxUploadPartSession> RemainParts => remainParts;
        TboxStartChunkUploadResDto uploadContext;
        private Pack<long> chunkProgress;
        public TboxUploadState State { get; set; }
        public long Progress
        {
            get
            {
                return chunkProgress.Value;
            }
        }

        public void ClearProgress()
        {
            chunkProgress.Value = 0;
        }

        public void Init(string path, long size)
        {
            this.path = path;
            this.size = size;
            chunkCount = size.GetChunkCount();
            remainParts = new List<TboxUploadPartSession>();
            chunkProgress = new Pack<long>(0);
            State = TboxUploadState.NotInit;
        }

        public void Init(string path, long size, string confirmKey, List<int> remainParts)
        {
            this.path = path;
            this.size = size;
            chunkCount = size.GetChunkCount();
            this.remainParts = remainParts.Select(x => new TboxUploadPartSession(x)).ToList();
            this.confirmKey = confirmKey;
            chunkProgress = new Pack<long>(0);
            State = TboxUploadState.ConfirmKeyInit;
        }

        public CommonResult<TboxErrorMessageDto> PrepareForUpload(CancellationToken ct)
        {
            if (State == TboxUploadState.Ready || State == TboxUploadState.Uploading || State == TboxUploadState.Done)
                return new CommonResult<TboxErrorMessageDto>(true, "");

            if (State == TboxUploadState.NotInit || State == TboxUploadState.Error)
            {
                var res = _tbox.StartChunkUpload(path, chunkCount, ct);
                if (!res.Success)
                {
                    if (res.Result != null)
                        return new CommonResult<TboxErrorMessageDto>(false, $"开始分块上传出错：{res.Message}", new TboxErrorMessageDto() { Code = res.Result?.Code, Message = res.Result?.Message, Status = res.Result.Status });
                    else
                        return new CommonResult<TboxErrorMessageDto>(false, $"开始分块上传出错：{res.Message}");
                }
                uploadContext = res.Result;
                confirmKey = uploadContext.ConfirmKey;
                for (int i = 1; i <= chunkCount; i++) remainParts.Add(new TboxUploadPartSession(i));
                State = TboxUploadState.Ready;
            }
            else //ConfirmKeyInit
            {
                if (remainParts.Count > 0)
                {
                    var res = _tbox.RenewChunkUpload(confirmKey, GetRefreshPartNumberList(), ct);
                    if (!res.Success)
                    {
                        if (res.Result != null)
                            return new CommonResult<TboxErrorMessageDto>(false, $"刷新分块凭据出错：{res.Message}", new TboxErrorMessageDto() { Code = res.Result?.Code, Message = res.Result?.Message, Status = res.Result.Status });
                        else
                            return new CommonResult<TboxErrorMessageDto>(false, $"刷新分块凭据出错：{res.Message}");
                    }
                    uploadContext = res.Result;
                }
                State = TboxUploadState.Ready;
            }
            return new CommonResult<TboxErrorMessageDto>(true, "");
        }

        public CommonResult<TboxUploadPartSession> GetNextPartNumber()
        {
            if (State != TboxUploadState.Ready && State != TboxUploadState.Uploading)
                return new CommonResult<TboxUploadPartSession>(false, "非法状态");

            if (remainParts.Count == 0)
                return new CommonResult<TboxUploadPartSession>(true, "已完成", new TboxUploadPartSession(0));

            var part = remainParts.FirstOrDefault(x => x.Uploading != true);
            if (part == null)
            {
                return new CommonResult<TboxUploadPartSession>(true, "等待完成", new TboxUploadPartSession(0));
            }
            part.Uploading = true;
            return new CommonResult<TboxUploadPartSession>(true, "", part);
        }

        public void ResetPartNumber(TboxUploadPartSession part)
        {
            part.Uploading = false;
        }

        public CommonResult EnsureDirectoryExists(CancellationToken ct)
        {
            if (State == TboxUploadState.Ready || State == TboxUploadState.Uploading || State == TboxUploadState.Done || State == TboxUploadState.ConfirmKeyInit)
                return new CommonResult(true, "");

            var p = path.GetParentPath();
            if (p == "" || p == "/")
            {
                return new CommonResult(true, "");
            }
            var res = _tbox.CreateDirectory(p, ct);
            if (res.Success)
                return new CommonResult(true, "");
            if (res.Result == null)
                return new CommonResult(false, $"创建文件夹出错：{res.Message}");
            if (res.Result.Code == "SameNameDirectoryOrFileExists")
                return new CommonResult(true, "");
            return new CommonResult(false, $"创建文件夹出错：{res.Message}");
        }

        public CommonResult EnsureNoExpire(int partNumber, CancellationToken ct)
        {
            var exp = uploadContext.Expiration;
            if ((exp - DateTime.Now).TotalSeconds < 30)
            {
                var res = _tbox.RenewChunkUpload(confirmKey, GetRefreshPartNumberList(), ct);
                if (!res.Success)
                    return new CommonResult(false, $"刷新分块凭据出错：{res.Message}");
                uploadContext = res.Result;
            }
            if (!uploadContext.Parts.ContainsKey(partNumber.ToString()))
            {
                var res = _tbox.RenewChunkUpload(confirmKey, GetRefreshPartNumberList(), ct);
                if (!res.Success)
                    return new CommonResult(false, $"刷新分块凭据出错：{res.Message}");
                uploadContext = res.Result;
            }
            if (!uploadContext.Parts.ContainsKey(partNumber.ToString()))
                return new CommonResult(false, $"已刷新上传凭据，但是未找到块 {partNumber} 的信息");
            return new CommonResult(true, "");
        }

        public CommonResult Upload(MemoryStream data, int partNumber, CancellationToken ct)
        {
            var res = _tbox.UploadChunk(uploadContext, data, partNumber, chunkProgress, ct);
            if (res.Success)
            {
                return new CommonResult(true, "");
            }
            else
            {
                return new CommonResult(false, res.Message);
            }
        }

        public CommonResult<TboxConfirmUploadResDto> Confirm(ulong crc64, CancellationToken ct)
        {
            //Todo:再次请求检查是否有未上传
            var res = _tbox.ConfirmUpload(confirmKey, crc64, ct);
            if (!res.Success)
                return new (false, $"确认上传出错：{res.Message}");
            return res;
        }

        public void CompletePart(TboxUploadPartSession part)
        {
            remainParts.Remove(part);
        }

        private List<int> GetRefreshPartNumberList()
        {
            return remainParts.Take(50).Select(x => x.PartNumber).ToList();
        }

    }

    public class TboxUploadPartSession
    {
        public TboxUploadPartSession(int partNumber)
        {
            PartNumber = partNumber;
            Uploading = false;
        }

        public int PartNumber { get; set; }
        public bool Uploading { get; set; }
    }

    public enum TboxUploadState
    {
        NotInit = 0,
        ConfirmKeyInit = 1,
        Ready = 2,
        Uploading = 3,
        Done = 4,
        Error = 5
    }
}
