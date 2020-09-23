using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Model
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public struct PageInfo
    {
        /// <summary>
        /// 分页的每页显示数量,若 等于 -1 ，则代表空分页
        /// </summary>
        [BsonElement("pageSize")]
        public int PageSize { get; set; }
        /// <summary>
        /// 分页的序号，从1开始
        /// </summary>
        [BsonElement("pageIndex")]
        public int PageIndex { get; set; }
        /// <summary>
        /// 排序规则 :格式： Model属性名 [asc|desc]  ，默认asc可以不写，eg: id,name desc 
        /// </summary>
        [BsonElement("orderBy")]
        public string OrderBy { get; set; }
        public PageInfo(int pageSize = -1, int pageIndex = 1, string orderBy = "")
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            OrderBy = orderBy;
        }
        //public PageInfo()
        //{
        //}
        public long StartPage
        {
            get
            {
                if (PageSize == 0)
                {
                    return 0;
                }
                else
                {
                    if (PageIndex <= 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return (PageIndex - 1) * PageSize;
                    }
                }
            }
        }
        public long EndPage
        {
            get
            {
                if (PageSize == 0)
                {
                    return long.MaxValue;
                }
                else
                {
                    if (PageIndex - 1 == 0)
                    {
                        return PageSize;
                    }
                    else
                    {
                        return (PageIndex - 1) * PageSize + PageSize;
                    }
                }
            }
        }

        public void SetDefault()
        {
            PageIndex = PageIndex < 1 ? 1 : PageIndex;
            PageSize = PageSize <= 0 ? int.MaxValue : PageSize;
        }
    }
}
