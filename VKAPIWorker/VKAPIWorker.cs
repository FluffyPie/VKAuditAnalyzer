using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace VKAPIWorker
{
    public static class VKAPIWorkerClass
    {
        const string access_token = "ec0883b0ec0883b0ec0883b006ec7c02e5eec08ec0883b0b38650e906234cbcb56e6ad6";

        static Uri GetUrlRequest(string method, string oauth, string property) =>
            new Uri($"https://api.vk.com/method/{ method }?{ property }&oauth={ oauth }&access_token={ access_token }&lang=0&v=5.124");

        static JsonElement GetRespons(Uri request)
        {
            using WebClient wc = new WebClient();
            JsonDocument respons = JsonDocument.Parse(wc.DownloadString(request));
            return respons.RootElement;
        }

        static Dictionary<string, int> GetMemberActivities(int user_id, string oauth)
        {
            const int MAX_GROUP_COUNT = 350;
            Uri url_groups_request = GetUrlRequest("users.getSubscriptions", oauth, $"user_id={ user_id }&extended=0");
            JsonElement member_sub = GetRespons(url_groups_request);
            if (member_sub.TryGetProperty("error", out JsonElement error))
            {
                int code_error = error.GetProperty("error_code").GetInt32();
                if (code_error == 30)
                    return null;
            }

            int[] groups = member_sub.GetProperty("response").GetProperty("groups").GetProperty("items").EnumerateArray().Select(id => id.GetInt32()).ToArray();
            int array_count = groups.Length / MAX_GROUP_COUNT + 1;
            List<string> activities = new List<string>();
            int group_offset = 0;
            for (int i = 0; i < array_count; i++)
            {
                string temp_ids = string.Empty;
                for (int j = 0 + group_offset; j < MAX_GROUP_COUNT + group_offset; j++)
                {
                    if (j >= groups.Length)
                        break;
                    temp_ids += groups[j] + ",";
                }
                if (temp_ids.Length == 0)
                    return null;

                temp_ids = temp_ids.Remove(temp_ids.Length - 1);
                group_offset += MAX_GROUP_COUNT;

                Uri url_activity_request = GetUrlRequest("groups.getById", oauth, $"group_ids={ temp_ids }&fields=activity");
                foreach (var group_info in GetRespons(url_activity_request).GetProperty("response").EnumerateArray())
                    if (group_info.TryGetProperty("activity", out JsonElement activity))
                        activities.Add(activity.GetString());
            }
            Dictionary<string, int> dic_activities = new Dictionary<string, int>();
            foreach (string activ in activities.Distinct())
                dic_activities.Add(activ, activities.Where(t => t == activ).Count());
            return dic_activities;
        }

        public static bool TryGetGroupId(out string value, string group_url)
        {
            value = string.Empty;
            try
            {                
                if (string.IsNullOrEmpty(group_url))
                    return false;
                string[] a = group_url.Split("/", StringSplitOptions.RemoveEmptyEntries);
                if (a.Length < 2)
                    return false;
                string screen_name = group_url.Contains("http") ? a[2] : a[1];
                JsonElement respons = GetRespons(GetUrlRequest("utils.resolveScreenName", "1", $"screen_name={screen_name}")).GetProperty("response");
                JsonElement type;
                if (respons.TryGetProperty("type", out type))
                {
                    if (type.ToString() == "group")
                    {
                        value = respons.GetProperty("object_id").ToString();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static Dictionary<string, int> GetActivities(string group_url, int members_limit = 0)
        {
            string oauth = "7c7f1f3a5ce490ffc8";            
            string respons = string.Empty;
            List<int> members = new List<int>();

            string group_id;
            if (!TryGetGroupId(out group_id, group_url))
                return null;

            Uri url_member_count_request = GetUrlRequest("groups.getById", oauth, $"group_id={ group_id }&fields=members_count");
            int member_count = GetRespons(url_member_count_request).GetProperty("response")[0].GetProperty("members_count").GetInt32();

            if (members_limit > 0 && member_count > members_limit)
                member_count = members_limit;      

            using BlockingCollection<int> block_members = new BlockingCollection<int>(member_count);
            List<Uri> url_members_requests = new List<Uri>();
            for (int offset = 0; offset < member_count; offset += 1000)
            {
                Uri url_members_request = GetUrlRequest("groups.getMembers", oauth, $"group_id={ group_id }&offset={ offset }");
                url_members_requests.Add(url_members_request);
            }

            foreach (var url in url_members_requests)
            {
                new Thread(() =>
                {
                    var list = GetRespons(url).GetProperty("response").GetProperty("items").EnumerateArray().Select(id => id.GetInt32());
                    foreach (var item in list)
                        block_members.Add(item);
                }).Start();
            }

            while (block_members.Count() != member_count) { }

            var dic_activities = new ConcurrentDictionary<string, int>();
            List<Thread> threads = new List<Thread>();
            foreach (var member in block_members)
            {
                threads.Add(new Thread(() =>
                {
                    var member_activity = GetMemberActivities(member, oauth);
                    if (member_activity != null)                    
                        foreach (var activity in member_activity)                        
                            if (!dic_activities.TryAdd(activity.Key, activity.Value))
                                dic_activities[activity.Key] += activity.Value;
                }));
            }

            List<Thread> started_threads = new List<Thread>();
            List<Thread> end_work_threads = new List<Thread>();
            for (int i = 0;  threads.Count != 0;)
            {
                foreach (var item in threads)
                {
                    if (i++ % 250 == 0 && i != 0) break;
                        item.Start();
                    started_threads.Add(item);
                }
                while (started_threads.Count != 0)
                {
                    foreach (var item in started_threads)
                        if (!item.IsAlive)
                            end_work_threads.Add(item);

                    foreach (var item in end_work_threads)
                    {
                        threads.Remove(item);
                        started_threads.Remove(item);
                    }
                    end_work_threads.Clear();
                    Thread.Sleep(100);
                }
            }

            return dic_activities
                .Where(x => !x.Key.Contains("Данный материал заблокирован на территории РФ"))
                .OrderBy(x => x.Value)                
                .Reverse()
                .Take(10)
                .ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
