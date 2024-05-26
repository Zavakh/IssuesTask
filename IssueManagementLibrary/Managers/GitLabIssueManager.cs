using IssueManagementLibrary.Managers;
using IssueManagementLibrary.Models;
using Newtonsoft.Json;
using System.Text;

namespace IssueManagerLibrary
{
    public class GitLabIssueManager : IssueManager
    {
        private readonly string _projectId;
        private readonly string _token;

        public GitLabIssueManager(string projectId, string token)
        {
            _projectId = projectId;
            _token = token;
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Private-Token", _token);
            return client;
        }

        public override async Task<ResponseEnum> AddIssueAsync(IssueModel issue)
        {
            using (var client = CreateHttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                var json = JsonConvert.SerializeObject(new { title = issue.Title, description = issue.Body != null ? issue.Body : issue.Description });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://gitlab.com/api/v4/projects/{_projectId}/issues", content);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ResponseEnum.UNAUTHORIZED;
                }
                response.EnsureSuccessStatusCode();
                return ResponseEnum.OK;
            }
        }

        public override async Task<ResponseEnum> UpdateIssueAsync(int issueId, IssueModel issue)
        {
            using (var client = CreateHttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                var json = JsonConvert.SerializeObject(new { title = issue.Title, description = issue.Body != null ? issue.Body : issue.Description });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PutAsync($"https://gitlab.com/api/v4/projects/{_projectId}/issues/{issueId}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ResponseEnum.UNAUTHORIZED;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ResponseEnum.NOT_FOUND;
                }
                response.EnsureSuccessStatusCode();
                return ResponseEnum.OK;
            }
        }

        public override async Task<ResponseEnum> CloseIssueAsync(int issueId)
        {
            using (var client = CreateHttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                var json = JsonConvert.SerializeObject(new { state_event = "close" });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"https://gitlab.com/api/v4/projects/{_projectId}/issues/{issueId}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ResponseEnum.UNAUTHORIZED;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ResponseEnum.NOT_FOUND;
                }
                response.EnsureSuccessStatusCode();
                return ResponseEnum.OK;
            }
        }

        public override async Task<List<IssueModel>> GetAllIssuesAsync()
        {
            using (var client = CreateHttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                var response = await client.GetStringAsync($"https://gitlab.com/api/v4/projects/{_projectId}/issues");

                var issues = JsonConvert.DeserializeObject<List<IssueModel>>(response);
                return issues;
            }
        }

        public override async Task ImportIssuesAsync(string filePath)
        {
            var issues = JsonConvert.DeserializeObject<List<IssueModel>>(System.IO.File.ReadAllText(filePath));
                foreach (var issue in issues)
                {
                    await AddIssueAsync(issue);
                }
        }

        public override async Task ExportIssuesAsync(string filePath)
        {
            var issues = await GetAllIssuesAsync();
            var json = JsonConvert.SerializeObject(issues, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, json);
        }
    }
}
