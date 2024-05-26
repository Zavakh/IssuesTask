using IssueManagementLibrary.Models;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace IssueManagementLibrary.Managers
{
    public class GitHubIssueManager : IssueManager
    {
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly string _token;

        public GitHubIssueManager(string repoOwner, string repoName, string token)
        {
            _repoOwner = repoOwner;
            _repoName = repoName;
            _token = token;
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"token {_token}");
            return client;
        }

        public override async Task<ResponseEnum> AddIssueAsync(IssueModel issue)
        {
            using (var client = CreateHttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                var json = JsonConvert.SerializeObject(new { title = issue.Title, body = issue.Body != null ? issue.Body : issue.Description });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://api.github.com/repos/{_repoOwner}/{_repoName}/issues", content);

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
                var json = JsonConvert.SerializeObject(new { title = issue.Title, body = issue.Body != null ? issue.Body : issue.Description });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"https://api.github.com/repos/{_repoOwner}/{_repoName}/issues/{issueId}", content);
                if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden) 
                {
                    return ResponseEnum.UNAUTHORIZED;
                }
                if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                var json = JsonConvert.SerializeObject(new { state = "closed" });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"https://api.github.com/repos/{_repoOwner}/{_repoName}/issues/{issueId}", content);
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
                var response = await client.GetStringAsync($"https://api.github.com/repos/{_repoOwner}/{_repoName}/issues");

                var issues = JsonConvert.DeserializeObject<List<IssueModel>>(response);
                return issues;                
            }
        } 

        public override async Task ImportIssuesAsync(string filePath)
        {
            var issues = JsonConvert.DeserializeObject<List<IssueModel>>(File.ReadAllText(filePath));
                foreach (var issue in issues)
                {
                    await AddIssueAsync(issue);
                }
        }

        public override async Task ExportIssuesAsync(string filePath)
        {
            var issues = await GetAllIssuesAsync();
            var json = JsonConvert.SerializeObject(issues, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
