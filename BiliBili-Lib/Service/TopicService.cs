﻿using BiliBili_Lib.Models.BiliBili;
using BiliBili_Lib.Models.Others;
using BiliBili_Lib.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili_Lib.Service
{
    public class TopicService
    {
        /// <summary>
        /// 获取话题动态
        /// </summary>
        /// <param name="topicId">话题ID</param>
        /// <param name="topicName">话题名</param>
        /// <param name="offset">偏移值，初次不需要，每次请求会返回下一次请求的偏移值</param>
        /// <returns>Item1:下次偏移值;Item3:视频列表</returns>
        public async Task<Tuple<string, List<Topic>>> GetTopicAsync(int topicId, string topicName, string offset = "")
        {
            var param = new Dictionary<string, string>();
            param.Add("topic_id", topicId.ToString());
            param.Add("topic_name", Uri.EscapeDataString(topicName));
            //param.Add("video_meta", "qn:32,fnval:16,fnver:0,fourk:1");
            if (!string.IsNullOrEmpty(offset))
                param.Add("offset", offset);
            string url = BiliTool.UrlContact(Api.TOPIC_COMPLEX, param, true);
            var data = await BiliTool.GetTextFromWebAsync(url);
            if (!string.IsNullOrEmpty(data))
            {
                var jobj = JObject.Parse(data);
                string nextOffset = jobj["offset"].ToString();
                var topics = JsonConvert.DeserializeObject<List<Topic>>(jobj["cards"].ToString());
                topics.RemoveAll(p => p == null || p.card == null || p.card.Length < 10);
                return new Tuple<string, List<Topic>>(nextOffset, topics);
            }
            return null;
        }
        /// <summary>
        /// 设置动态点赞状态
        /// </summary>
        /// <param name="isLike">是否点赞</param>
        /// <param name="dynamicId">动态ID</param>
        /// <param name="rid">动态里的参数</param>
        /// <param name="uid">用户ID</param>
        /// <returns></returns>
        public async Task<bool> SetDynamicLikeStatus(bool isLike, string dynamicId, string rid, string uid)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(dynamicId) || string.IsNullOrEmpty(rid))
                return false;
            var param = new Dictionary<string, string>();
            param.Add("dynamic_id", dynamicId);
            param.Add("rid", rid);
            param.Add("uid", uid);
            param.Add("up", isLike ? "1" : "2");
            var data = BiliTool.UrlContact("", param, true);
            var response = await BiliTool.PostContentToWebAsync(Api.DYNAMIC_LIKE, data);
            if (!string.IsNullOrEmpty(response))
            {
                var jobj = JObject.Parse(response);
                if (jobj["code"].ToString() == "0")
                    return true;
            }
            return false;
        }
    }
}