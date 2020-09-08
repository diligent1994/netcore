using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Common.Model
{
    /// <summary>
    /// 所有api请求公共返回体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultObject<T> : IActionResult
    {
        #region Fileds

        protected static JsonWriterSettings jsonWriterSettings = new JsonWriterSettings() { OutputMode = JsonOutputMode.Strict, MaxSerializationDepth = 1000 };

        protected HttpRequest _request;

        const int DataBufferSize = 50 * 1024 * 1024;   //每次分批加载的50m
        const int DataMaxSize = 250 * 1024 * 1024;      //文件超过250m，进行分批加载操作，否则一次加载
        #endregion

        #region Property

        [JsonProperty("status")]
        [BsonElement("status")]
        [DataMember(Name = "status")]
        public virtual int Status { get; set; }

        [BsonIgnoreIfNull]
        [JsonProperty("message")]
        [BsonElement("message")]
        [DataMember(Name = "message")]
        public virtual string Message { get; set; }


        [JsonProperty("data")]
        [BsonElement("data")]
        [DataMember(Name = "data")]
        public virtual T Data { get; set; }
        #endregion

        #region Constructor

        public ResultObject( T data)
        {
            this.Status = 0;
            this.Message = null;
            this.Data = data;
        }

        public ResultObject(HttpRequest request, T data)
        {
            this._request = request;
            this.Status = 0;
            this.Message = null;
            this.Data = data;
        }

        public ResultObject(HttpRequest request, int status, string message, T data = default(T))
            : this(request, data)
        {
            this.Status = status;
            this.Message = message;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// core版本的接口实现ExecuteResultAsync方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "text/plain;charset=utf-8";
            
            //若是byte[]集合，则特殊处理，不需要序列化操作
            if (Data is byte[])
            {
                var data = Data as byte[];
                return WriteByteAsync(context, data);
            }
            else if (Data is BsonDocument)
            {
                var data = Data as BsonDocument;
                return context.HttpContext.Response.WriteAsync(data.ToJson());
            }

            var content = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            return context.HttpContext.Response.WriteAsync(content);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static Task WriteByteAsync(ActionContext context, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return WriteByte(context, ms, data);
            }
        }

        public static Task WriteFile(ActionContext context, string targetFile)
        {
            using (FileStream fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
            {
                return WriteByte(context, fs);
            }
        }


        #endregion

        #region Private Methods
        /// <summary>
        /// 内容输出 ，支持文件和 object对象的byte[] ，且支持大于500M以上的大对象（如果直接输出iis会异常） 
        /// 改成批量加载
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="stream">MemoryStream或者FileStream类型</param>
        /// <param name="data">stream是FileStream，则data参数省略</param>
        private static Task WriteByte(ActionContext context, Stream stream, byte[] data = null)
        {
            long contentLength = stream.Length; //获取数据流的大小
            if (contentLength < DataMaxSize)   //若小于DataMaxSize，则一次加载
            {
                if (stream is FileStream)   //如果文件，则从文件加载，不是传入data
                {
                    var streamContent = new StreamContent(stream as FileStream);
                    data = streamContent.ReadAsByteArrayAsync().Result;
                }
                
                return context.HttpContext.Response.Body.WriteAsync(data, 0, data.Length);
            }
            var response = context.HttpContext.Response;

            using (response.Body)//调用Response.Body.Dispose()并不会关闭客户端浏览器到ASP.NET Core服务器的连接，之后还可以继续往Response.Body中写入数据
            {
                response.ContentLength = contentLength;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

                byte[] buffer;
                long hasRead = 0;//变量hasRead用于记录已经发送了多少字节的数据到客户端浏览器

                //如果hasRead小于contentLength，说明下载文件还没读取完毕，继续循环读取下载文件的内容，并发送到客户端浏览器
                while (hasRead < contentLength)
                {
                    //HttpContext.RequestAborted.IsCancellationRequested可用于检测客户端浏览器和ASP.NET Core服务器之间的连接状态，如果HttpContext.RequestAborted.IsCancellationRequested返回true，说明客户端浏览器中断了连接
                    if (context.HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        //如果客户端浏览器中断了到ASP.NET Core服务器的连接，这里应该立刻break，取消下载文件的读取和发送，避免服务器耗费资源
                        break;
                    }
                    buffer = new byte[DataBufferSize];
                    int currentRead = stream.Read(buffer, 0, DataBufferSize);//从数据流中读取bufferSize大小的内容到服务器内存中
                    response.Body.Write(buffer, 0, currentRead);    //发送读取的内容数据到客户端浏览器
                    response.Body.Flush();          //注意每次Write后，要及时调用Flush方法，及时释放服务器内存空间

                    hasRead += currentRead;//更新已经发送到客户端浏览器的字节数
                }
            }
            return Task.CompletedTask;
        }
        #endregion
    }

    /// <summary>
    /// data是string类型的公共返回体
    /// </summary>
    public class StringResultObject : ResultObject<string>
    {

        #region Constructor
        public StringResultObject() : this(null)
        {
        }

        public StringResultObject(HttpRequest request, string data = null)
            : base(request, data)
        {
            // Nothing to do.
        }

        public StringResultObject(HttpRequest request, int success, string message, string data = null)
            : base(request, success, message, data)
        {
            // Nothing to do.
        }

        #endregion
    }
}
