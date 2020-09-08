using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using System.Text;
using MongoDB.Bson.Serialization;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.IO;
using MongoDB.Bson.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Common.Infrastructure;

namespace Common.Util
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        #region field

        public const string Regex_IsEmail = "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$";
        public const string Regex_Phone = "^[1-9][0-9]{10}$";
        public const string Regex_16 = "\\\\u";
        public const string Regex_FileNameInvalid = @"(/)|(\\)|(\:)|(\*)|(\?)|(\|)|(<)|(>)|" + "(\")";  //文件名非法字符
        /// <summary>
        /// mongodb 认证数据库
        /// </summary>
        public const string DEFAULT_AuthenticationDatabase = "admin";
        /// <summary>
        /// mongodb 认证模式
        /// </summary>
        public const string DEFAULT_AuthenticationMechanism = "SCRAM-SHA-1";
        #endregion

        /// <summary>
        /// Array 集合判断是否空 [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this Array list)
        {
            return list == null || list.Length == 0;
        }
        /// <summary>
        /// IEnumerable<T> 判定是否空 [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || list.Count() == 0;
        }

        public static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return dict == null || dict.Count() == 0;
        }

        /// <summary>
        /// value是否存在Array中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="list">The Array.</param>
        /// <returns>true:存在 flase:未存在</returns>
        public static bool Contain<T>(this T value, params T[] list)
        {
            if (list.IsNullOrEmpty()) return false;

            return Array.IndexOf(list, value) >= 0;
        }

        /// <summary>
        /// value是否存在list中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="list">The list.</param>
        /// <returns>true:存在 flase:未存在</returns>
        public static bool Contain<T>(this T value, IEnumerable<T> list)
        {
            if (list.IsNullOrEmpty()) return false;

            return list.Contains(value);
        }

        public static bool ContainList(this string[] list, string value, bool isIgnoreCase = true)   //isIgnoreCase:是否忽略大小写，默认忽略
        {
            if (list.IsNullOrEmpty() || value == null) return false;
            if (isIgnoreCase)
                list = list.Select(s => s.ToLowerInvariant()).ToArray();
            return list.Contains(value);
        }

        public static bool ContainList(this IEnumerable<string> list, string value, bool isIgnoreCase = true)
        {
            if (list.IsNullOrEmpty() || value == null) return false;
            if (isIgnoreCase)
                list = list.Select(s => s.ToLowerInvariant());
            return list.Contains(value);
        }

        /// <summary>
        /// 获取集合索引值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="notIndex"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this T[] list, T obj)
        {
            return Array.IndexOf(list, obj);
        }

        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

        /// <summary>
        /// 若是科学计算法的，改成 纯数字
        /// </summary>
        /// <param name="scientificCalExp"></param>
        /// <returns></returns>
        public static decimal CastScientificCal(this string scientificCalExp)
        {
            if (string.IsNullOrEmpty(scientificCalExp)) return 0;
            if (!scientificCalExp.Contains("E")) return Convert.ToDecimal(scientificCalExp);

            return Convert.ToDecimal(Decimal.Parse(scientificCalExp, System.Globalization.NumberStyles.Float));
        }

        public static string[] SplitContent(this string str, char splitValue, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return str.Split(splitValue, options);
        }

        /// <summary>
        /// 字符串分割（Split封装)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string[] SplitContent(this string str, string splitValue, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return str.Split(new string[] { splitValue }, options);
        }

        public static string ToX16(this string str)
        {
            str = str.Replace(@"\\u", @"\u");
            return str;
        }



        /// <summary>
        /// utf-8的字符串改成Shift-JIS，日本字体：如： \u30fc 转换成日文文本
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ToShiftJis(this string content)
        {
            Encoding orginal = Encoding.GetEncoding("utf-8");
            Encoding ShiftJis = Encoding.GetEncoding("Shift-JIS");
            byte[] unf8Bytes = orginal.GetBytes(content);
            byte[] myBytes = Encoding.Convert(orginal, ShiftJis, unf8Bytes);
            string JISContent = ShiftJis.GetString(myBytes);
            return JISContent;
        }

        /// <summary>
        ///  搜索str出现search字符串的次数，大小写忽略
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static int SearchNum(this string str, string search)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(search)) return 0;

            var matches = Regex.Matches(str, search, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var count = matches.Count;
            return count;
        }

        public static char[] RegexKeyChar = @"\~!@#$%^&*()+{}:|<>?[],./;""'".ToCharArray();   //必须\第一个替换

        /// <summary>
        /// 替换特殊字符成,\特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static string ReplaceText(this string str, char[] chars, string replaceText = null)
        {
            string replaceTxt = "";
            foreach (var text in chars)
            {
                var pattern = text.ToString();
                if (str.Contains(pattern))
                {
                    replaceTxt = replaceText;
                    if (replaceText == null) replaceTxt = @"\" + pattern;

                    str = Regex.Replace(str, @"\" + pattern, replaceTxt, RegexOptions.IgnoreCase);
                }
            }

            return str;
        }

        /// <summary>
        /// 替换特殊字符成,\特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static string ReplaceText(this string str, string[] inputs, string replaceText = "")
        {
            foreach (var text in inputs)
            {
                str = Regex.Replace(str, text, replaceText, RegexOptions.IgnoreCase);
            }

            return str;
        }

        /// <summary>
        /// 复制操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Clone<T>(this object obj)
            where T : class
        {
            return BsonSerializer.Deserialize<T>(obj.ToJson());
            //return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, Formatting.Indented,
            //    new JsonSerializerSettings
            //    {
            //        TypeNameHandling = TypeNameHandling.All
            //    }));
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(this T obj, JsonConvertType type = JsonConvertType.MongoBson)
        //where T : class 
        {
            if (obj == null)
                return null;

            string content = null;
            if (type == JsonConvertType.MongoBson)
                content = EngineContext.Current.Resolve<IMongoBsonSerializer<T>>().Serializer(obj);
            else if (type == JsonConvertType.JsonConvert)
                content = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Local,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    NullValueHandling = NullValueHandling.Ignore
                });
            else if (type == JsonConvertType.Utf8Json)
                content = Utf8Json.JsonSerializer.ToJsonString(obj);
            else if (type == JsonConvertType.NetCore)
                content = System.Text.Json.JsonSerializer.Serialize(obj,
                    new Text.Json.JsonSerializerOptions() { IgnoreNullValues = true, MaxDepth = 100 });
            return content;
        }

        /// <summary>
        /// 对象序列化成byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] SerializeObjectBytes<T>(this T obj, JsonConvertType type = JsonConvertType.MongoBson)
        //where T : class 
        {
            if (obj == null)
                return null;

            byte[] content = null;
            if (typeof(T).Name == "BsonDocument")
            {
                string bsonContent = (obj as BsonDocument).ToJson();
                return Encoding.UTF8.GetBytes(bsonContent);
            }
            if (type == JsonConvertType.MongoBson)
                content = obj.ToBson(typeof(T), new BsonBinaryWriterSettings()
                {
                    Encoding = new UTF8Encoding(),
                    MaxSerializationDepth = 1000
                });
            else if (type == JsonConvertType.JsonConvert)
            {
                string objContent = obj.SerializeObject(type);
                return Encoding.UTF8.GetBytes(objContent);
            }
            else if (type == JsonConvertType.Utf8Json)
            {
                content = Utf8Json.JsonSerializer.Serialize(obj);
            }
            else if (type == JsonConvertType.NetCore)
                content = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj,
                    new Text.Json.JsonSerializerOptions() { IgnoreNullValues = true, MaxDepth = 100 });
            return content;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(this string json, JsonConvertType type = JsonConvertType.MongoBson)
        //where T : class
        {
            T obj = default(T);
            if (type == JsonConvertType.MongoBson)
                obj = EngineContext.Current.Resolve<IMongoBsonSerializer<T>>().Deserialize(json);
            else if (type == JsonConvertType.JsonConvert)
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Local,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                });
            else if (type == JsonConvertType.Utf8Json)
                obj = Utf8Json.JsonSerializer.Deserialize<T>(json);
            else if (type == JsonConvertType.NetCore)
                obj = System.Text.Json.JsonSerializer.Deserialize<T>(json);
            return obj;
        }

        public static Dictionary<TKey, TValue> Filter<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            return dict.Where(predicate).ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// 根据key获取Dic的value值
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultVal">找不到的默认值，可空</param>
        /// <returns></returns>
        public static TValue GetValueByKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultVal = default(TValue))
        {
            if (dict.IsNullOrEmpty()) return defaultVal;

            return dict.ContainsKey(key) ? dict[key] : defaultVal;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ChangeType<T>(this object obj)
        {
            if (obj == null) return default(T);

            return (T)Convert.ChangeType(obj.ToString(), typeof(T));
        }

        /// <summary>
        /// 递归获取错误子错误
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception GetInnerException(this Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnerException(ex.InnerException);

            return ex;
        }

        public static string ToStringDt(this DateTime dt, string fomate = "yyyy-MM-dd HH:mm:ss")
        {
            if (fomate == "c")
                fomate = "yyyy/MM/dd";
            else if (fomate == "C")
                fomate = "yyyy/MM/dd HH:mm:ss";
            else if (fomate == "n")
                fomate = "yyyy_MM_dd";
            else if (fomate == "N")
                fomate = "yyyy_MM_dd HH_mm_ss";
            return dt.ToString(fomate, System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// 是否存在Attribute，net 和Core版本都兼容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public static bool IsExistsAttribute<T>(
#if !NETCOREAPP
            this HttpActionDescriptor actionDescriptor
#else
            this ActionDescriptor actionDescriptor
#endif
        )
            where T : Attribute
        {
#if !NETCOREAPP
            return actionDescriptor.GetCustomAttributes<T>().
                    OfType<T>().
                    Any(a => a is T);

#else
            var descriptor = ((ControllerActionDescriptor)actionDescriptor);
            return descriptor.ControllerTypeInfo.IsDefined(typeof(T), true) || descriptor.MethodInfo.IsDefined(typeof(T), true);
#endif
        }

        #region ，DataRow相关操作
        public static T GetValue<T>(this DataRow row, int index)
        {
            var value = row[index];
            if (value is DBNull)
                return default(T);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetValue(this DataRow row, int index)
        {
            return row.GetValue<string>(index);
        }

        public static string GetValue(this DataRow row, string colName)
        {
            return row.GetValue<string>(colName);
        }

        public static T GetValue<T>(this DataRow row, string colName)
        {
            var value = row[colName];
            if (value is DBNull)
                return default(T);
            return (T)Convert.ChangeType(value, typeof(T));
        }
        #endregion

        #region 格式判断
        /// <summary>
        /// 是否邮箱地址
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string email)
        {
            return Check(email, Regex_IsEmail);
        }

        /// <summary>
        /// 是否手机号格式
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool IsPhone(this string phone)
        {
            return Check(phone, Regex_Phone);
        }

        public static bool Check(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            if (String.IsNullOrEmpty(input))
                return false;

            input = input.Trim();
            var result = Regex.IsMatch(input, pattern, options);
            return result;
        }

        /// <summary>
        /// 文件名替换一下非法字符
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="replacement">默认值：非法字符替换成下划线"_"</param>
        /// <returns></returns>
        public static string ReplaceInvalid(this string fileName, string replacement = "_")
        {
            Regex rgx = new Regex(Regex_FileNameInvalid);
            return rgx.Replace(fileName, replacement);
        }

        #endregion

        #region Enum 枚举相关操作
        /// <summary>
        /// 获取的枚举的值的字符串值
        /// </summary>
        /// <param name="enumItem"></param>
        /// <returns></returns>
        public static string GetEnumValue(this Enum enumItem)
        {
            return (Convert.ToInt32(enumItem)).ToString();
        }
        #endregion

        #region BsonDocument相关操作
        public static BsonDocument ParseBson(this string json)
        {
            BsonDocument docBson = new BsonDocument();
            var ret = BsonDocument.TryParse(json, out docBson);
            if (ret == false)
                return null;
            return docBson;
        }

        /// <summary>
        /// 获取BsonDocument的name的值，若不存在，则返回null,值转换成字符串类型
        /// </summary>
        /// <param name="doc">BsonDocument对象</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValueString(this BsonDocument doc, string name)
        {
            if (doc == null) return null;
            BsonValue bVal = null;
            bool ret = doc.TryGetValue(name, out bVal);
            if (bVal == null) return null;

            return bVal.AsString;
        }

        /// <summary>
        /// 获取BsonDocument的name的值，若不存在，则返回null,值转换成T类型
        /// </summary>
        /// <param name="doc">BsonDocument对象</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetValue<T>(this BsonDocument doc, string name)
        {
            if (doc == null) return default(T);

            BsonValue bVal = null;
            bool ret = doc.TryGetValue(name, out bVal);
            if (bVal == null) return default(T);

            return (T)Convert.ChangeType(bVal.ToString(), typeof(T));
        }

        /// <summary>
        /// BsonValue 类型根据name获取string类型值，不会报错返回null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValueString(this BsonValue value, string name)
        {
            if (value == null) return null;

            BsonValue bVal = null;

            try
            {
                bVal = value[name];
            }
            catch
            {

            }

            if (bVal == null) return null;

            return bVal.AsString;
        }

        /// <summary>
        /// BsonValue 类型根据name获取值，不会报错 返回T默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetValue<T>(this BsonValue value, string name)
        {
            if (value == null) return default(T);

            BsonValue bVal = null;

            try
            {
                bVal = value[name];
            }
            catch
            {

            }

            if (bVal == null) return default(T);

            return (T)Convert.ChangeType(bVal.ToString(), typeof(T));
        }
        #endregion

        #region Jobject

        /// <summary>
        /// 根据key获取Jobject
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JObject GetJObject(this JObject jObject, object key)
        {
            JObject jobj = null;
            if (jObject[key] is JObject)
            {
                jobj = jObject[key].Value<JObject>();
            }
            else
            {
                string payload = jObject[key].Value<string>();
                jobj = JObject.Parse(payload);
            }
            return jobj;
        }

        /// <summary>
        /// 在JToken的内容里根据key获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this JToken value, object key)
        {
            if (value is JObject)
                return value[key].Value<T>();
            return JObject.Parse(value.ToString()).Value<T>(key);
        }

        /// <summary>
        /// 获取Jobject 中key的列表根据T泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<T> GetList<T>(this JObject jObject, object key)
        {
            if (key == null || !jObject.ContainsKey(key.ToString()) || !jObject[key].HasValues) return null;

            if (typeof(T).IsClass)
                return jObject.Value<JArray>(key).Select(s => s.ToObject<T>()).ToList();
            return jObject.Value<JArray>(key).Select(s => s.Value<T>()).ToList();
        }

        #endregion

        #region FilterDefinition

        public static FilterDefinition<TDocument> GetFilterIn<TDocument, TField>(this IEnumerable<object> list,
                         Expression<Func<TDocument, TField>> expression, IEnumerable<TField> values)
        {
            return Builders<TDocument>.Filter.In(expression, values);
        }

        #endregion

        #region Lambada Expression


        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// </summary>
        public static Func<T, TResult> GetFuncSetter<T, TProperty, TResult>(this Expression<Func<T, TProperty>> expression)
        {
            Expression<Func<T, TResult>> newExpression = Expression.Lambda<Func<T, TResult>>(
                Expression.Convert(expression.Body, typeof(TResult)),
                expression.Parameters);

            return newExpression.Compile();
        }
        #endregion

        #region Net Core的配置文档相关操作

        /// <summary>
        /// 加载Appsettings.json的，且注入到Container框架(单列模式）
        /// </summary>
        /// <typeparam name="TConfig">加载配置信息自定义类对象</typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns>返回配置TConfig</returns>
        public static TConfig ConfigureStartupConfig<TConfig>(this IServiceCollection services, IConfiguration configuration)
            where TConfig : class, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            var config = new TConfig();
            configuration.Bind(config);
            services.AddSingleton(config);
            return config;
        }

        public static IConfiguration CreateConfig(string jsonName = "appsettings.json")
        {
            var config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(jsonName, optional: true, reloadOnChange: true)
            .Build();
            return config;
        }

        /// <summary>
        /// 加载Appsettings.json的
        /// </summary>
        /// <typeparam name="TConfig">加载配置信息自定义类对象</typeparam>
        /// <param name="configuration"></param>
        /// <returns>返回配置TConfig</returns>
        public static TConfig ConfigureStartupConfig<TConfig>(this IConfiguration configuration)
            where TConfig : class, new()
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            var config = new TConfig();
            configuration.Bind(config);
            return config;
        }
        #endregion

        /// <summary>
        /// 删除ConcurrentDictionary 的hash 表的
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default(TValue);
            return dictionary.TryRemove(key, out value);
        }

        /// <summary>
        /// 获取DataTable的DataRow的List集合, 在 NetStardard2.1代码中会提示二义性错误，所有抽取出来
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<DataRow> GetRows(this DataTable dt)
        {
            return dt.AsEnumerable().ToList();
        }


        /// <summary>
        /// 合并路径 是ftp开头的 
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string CombineFtp(this string path1, string path2)
        {
            return Path.Combine(path1, path2).Replace("\\", "/");
        }

        /// <summary>
        /// 根据路径获取 磁盘名称 ，兼容linux 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDriveName(this string path)
        {
            if (ExtensionLinux.IsLinux)
            {
                return $"/{path.SplitContent("/")[0]}";
            }

            return path.Substring(0, 3);    //windows直接前面3位即可，d:\
        }

        /// <summary>
        /// 获取 文件夹实际路径， 兼容linux
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDictoryPath(this string path)
        {
            var replaceChar = ExtensionLinux.IsLinux ? '\\' : '/';
            return path.Replace(replaceChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 获取进程执行的命令行的参数
        /// </summary>
        /// <param name="process"></param>
        /// <param name="splitValue">参数分隔符，默认是空格</param>
        /// <returns></returns>
        public static string GetCommandLineArgs(this Process process, string splitValue = " ")
        {
            var cmd = process.GetCommandLine();
            var cmds = cmd?.SplitContent(splitValue);
            return cmds != null && cmds.Length > 1 ? cmds[1] : string.Empty;
        }

        /// <summary>
        /// 获取一个正在运行的进程的命令行。                
        /// </summary>
        /// <param name="process">一个正在运行的进程。</param>
        /// <returns>表示应用程序运行命令行的字符串。</returns>
        public static string GetCommandLine(this Process process)
        {
            if (process is null) throw new ArgumentNullException(nameof(process));

            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (var objects = searcher.Get())
                {
                    var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                    return @object?["CommandLine"]?.ToString() ?? "";
                }
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // 没有对该进程的安全访问权限。
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                // 进程已退出。
                return string.Empty;
            }
        }

        /// <summary>
        /// 执行文件，比如.bat,ps1,sh等
        /// </summary>
        /// <param name="fileName">处理语句</param>
        /// <param name="args">参数</param>
        /// <returns>返回值</returns>
        public static string RunProcess(this string fileName, string args = "", string workPath = "", bool isFile = false, bool isReadToEnd = true, bool isWaitForExit = true)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            if (isFile)  //若文件，则fileName是完整物理路径
                workPath = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(workPath))
                process.StartInfo.WorkingDirectory = workPath;
            process.Start();
            if (!isReadToEnd) return null;

            string result = process.StandardOutput.ReadToEnd();
            if (isWaitForExit)
                process.WaitForExit();
            return result;
        }

        /// <summary>
        /// 执行windows的bat文件
        /// </summary>
        /// <param name="fileFullName">bat完整物理路径文件</param>
        /// <param name="args"></param>
        /// <param name="isWaitForExit"></param>
        /// <returns></returns>
        public static string RunBat(this string fileFullName, string args = "", string workPath = "", bool isFile = false, bool isReadToEnd = true, bool isWaitForExit = false)
        {
            if (!File.Exists(fileFullName)) throw new Exception($"{fileFullName}文件不存在,无法执行");

            string result = fileFullName.RunProcess(args, workPath, isFile, isReadToEnd, isWaitForExit);
            return result;
        }

        #region Mongodb 

        /// <summary>
        /// 获取 mongodump.exe 和 mongorestore.exe 路径
        /// </summary>
        /// <param name="mongoExeName"></param>
        /// <returns></returns>
        public static string GetMongoToolEXEFilePath(string dirPath, string mongoExeName)
        {
            //linux不要.exe后缀
            if (ExtensionLinux.IsLinux) mongoExeName = Path.GetFileNameWithoutExtension(mongoExeName);

            string exePath = Path.Combine(dirPath, mongoExeName);
            if (!ExtensionLinux.IsLinux)
                exePath = "\"" + exePath + "\"";
            return exePath;
        }

        /// <summary>
        /// 执行 mongodump ，且兼容liunx
        /// </summary>
        /// <param name="host"></param>
        /// <param name="dbName"></param>
        /// <param name="colName"></param>
        /// <param name="filePath"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="authenticationDatabase"></param>
        /// <param name="authenticationMechanism"></param>
        /// <returns></returns>
        public static string GetDBDumpCmd(string host,
                                  string dbName,
                                  string colName,
                                  string filePath,
                                  BsonDocument filter = null,
                                  string user = null,
                                  string pwd = null,
                                  string authenticationDatabase = DEFAULT_AuthenticationDatabase,
                                  string authenticationMechanism = DEFAULT_AuthenticationMechanism)
        {
            string strCmd = string.Format("-h \"{0}\" -d \"{1}\" -c \"{2}\" -o \"{3}\"", host, dbName, colName, filePath);
            if (filter != null)
            {
                strCmd += string.Format(" --query \"{0}\"", filter.ToString());
            }
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                strCmd += string.Format(" -u \"{0}\" -p \"{1}\"", user, pwd);
                strCmd += string.Format(" --authenticationDatabase {0}", authenticationDatabase);
                strCmd += string.Format(" --authenticationMechanism {0}", authenticationMechanism);
            }

            return strCmd;
        }

        /// <summary>
        /// 执行mongorestore， 且兼容liunux
        /// </summary>
        /// <param name="host"></param>
        /// <param name="dbName"></param>
        /// <param name="colName"></param>
        /// <param name="dirPath"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="authenticationDatabase"></param>
        /// <param name="authenticationMechanism"></param>
        /// <returns></returns>
        public static string GetDBRestoreCmd(string host,
                                     string dbName,
                                     string colName,
                                     string dirPath,
                                     string user,
                                     string pwd,
                                     string authenticationDatabase = DEFAULT_AuthenticationDatabase,
                                     string authenticationMechanism = DEFAULT_AuthenticationMechanism)
        {
            string splitParam = !ExtensionLinux.IsLinux ? "/" : "-";

            string strCmd = string.Format("{0}h \"{1}\" {0}d \"{2}\" {0}c \"{3}\" --dir \"{4}\"", splitParam, host, dbName, colName, dirPath);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                strCmd += string.Format(" {0}u \"{1}\" {0}p \"{2}\"", splitParam, user, pwd);
                strCmd += string.Format(" --authenticationDatabase {0}", authenticationDatabase);
                strCmd += string.Format(" --authenticationMechanism {0}", authenticationMechanism);
            }

            strCmd += string.Format(" --maintainInsertionOrder");
            return strCmd;
        }

        #endregion
    }

    public enum JsonConvertType
    {
        /// <summary>
        /// 默认
        /// </summary>
        MongoBson = 0,
        JsonConvert,
        Utf8Json,
        /// <summary>
        /// 微软库中core3 版本序列化 ,NetFreamwork版本不支持
        /// </summary>
        NetCore
    }
}
