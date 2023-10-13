using JboxTransfer.Core.Helpers;
using JboxTransfer.Extensions;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
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

namespace JboxTransfer.Modules
{
    /// <summary>
    /// 用于管理上传整个文件的会话。支持由confirmKey进行断点续传
    /// </summary>
    public class TboxUploadSession
    {
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

        public TboxUploadSession(string path, long size) {
            this.path = path;
            this.size = size;
            this.chunkCount = size.GetChunkCount();
            this.remainParts = new List<TboxUploadPartSession>();
            this.chunkProgress = new Pack<long>(0);
            State = TboxUploadState.NotInit;
        }

        public TboxUploadSession(string path, long size, string confirmKey, List<int> remainParts)
        {
            this.path = path;
            this.size = size;
            this.chunkCount = size.GetChunkCount();
            this.remainParts = remainParts.Select(x => new TboxUploadPartSession(x)).ToList();
            this.confirmKey = confirmKey;
            this.chunkProgress = new Pack<long>(0);
            State = TboxUploadState.ConfirmKeyInit;
        }

        public CommonResult PrepareForUpload()
        {
            if (State == TboxUploadState.Ready || State == TboxUploadState.Uploading || State == TboxUploadState.Done)
                return new CommonResult(true, "");

            if (State == TboxUploadState.NotInit || State == TboxUploadState.Error)
            {
                var res = TboxService.StartChunkUpload(path, chunkCount);
                if (!res.Success)
                    return new CommonResult(false, $"开始分块上传出错：{res.Message}");
                uploadContext = res.Result;
                confirmKey = uploadContext.ConfirmKey;
                for (int i = 1; i <= chunkCount; i++) remainParts.Add(new TboxUploadPartSession(i));
                State = TboxUploadState.Ready;
            }
            else //ConfirmKeyInit
            {
                if (remainParts.Count > 0)
                {
                    var res = TboxService.RenewChunkUpload(confirmKey, GetRefreshPartNumberList());
                    if (!res.Success)
                        return new CommonResult(false, $"刷新分块凭据出错：{res.Message}");
                    uploadContext = res.Result;
                }
                State = TboxUploadState.Ready;
            }
            return new CommonResult(true, "");
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

        public CommonResult EnsureNoExpire(int partNumber)
        {
            var exp = DateTime.Parse(uploadContext.Expiration);
            if ((exp - DateTime.Now).TotalSeconds < 30)
            {
                var res = TboxService.RenewChunkUpload(confirmKey, GetRefreshPartNumberList());
                if (!res.Success)
                    return new CommonResult(false, $"刷新分块凭据出错：{res.Message}");
                uploadContext = res.Result;
            }
            if (!uploadContext.Parts.ContainsKey(partNumber.ToString()))
            {
                var res = TboxService.RenewChunkUpload(confirmKey, GetRefreshPartNumberList());
                if (!res.Success)
                    return new CommonResult(false, $"刷新分块凭据出错：{res.Message}");
                uploadContext = res.Result;
            }
            if (!uploadContext.Parts.ContainsKey(partNumber.ToString()))
                return new CommonResult(false, $"已刷新上传凭据，但是未找到块 {partNumber} 的信息");
            return new CommonResult(true, "");
        }

        public CommonResult Upload(MemoryStream data, int partNumber)
        {
            var headerInfo = uploadContext.Parts[partNumber.ToString()].Headers;

            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("uploadId", uploadContext.UploadId);
            query.Add("partNumber", partNumber.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("https://");
            sb.Append(uploadContext.Domain);
            sb.Append(uploadContext.Path);
            sb.Append(UrlHelper.BuildQuery(query));
            string url = sb.ToString();

            HttpWebRequest req = HttpWebRequest.CreateHttp(url);
            req.Method = "PUT";
            req.Accept = "*/*";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.Add("x-amz-date", headerInfo.XAmzDate);
            req.Headers.Add("authorization", headerInfo.Authorization);
            req.Headers.Add("x-amz-content-sha256", headerInfo.XAmzContentSha256);

            var reqstream = req.GetRequestStream();

            chunkProgress.Value = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(81920 / 2);
            try
            {
                int bytesWrite;
                while ((bytesWrite = data.Read(buffer, 0, buffer.Length)) != 0)
                {
                    reqstream.Write(buffer, 0, bytesWrite);
                    chunkProgress.Value += bytesWrite;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            try
            {
                var resp = req.GetResponse();
                return new CommonResult(true, "");
            }
            catch(WebException ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult<TboxConfirmChunkUploadResDto> Confirm(ulong crc64)
        {
            //Todo:再次请求检查是否有未上传
            var res = TboxService.ConfirmChunkUpload(confirmKey, crc64);
            if (!res.Success)
                return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"确认上传出错：{res.Message}");
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
