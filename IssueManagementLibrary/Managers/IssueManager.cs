using IssueManagementLibrary.Models;

namespace IssueManagementLibrary.Managers
{
    public abstract class IssueManager
    {
        public abstract Task<ResponseEnum> AddIssueAsync(IssueModel issue);
        public abstract Task<ResponseEnum> UpdateIssueAsync(int issueId, IssueModel issue);
        public abstract Task<ResponseEnum> CloseIssueAsync(int issueId);
        public abstract Task<List<IssueModel>> GetAllIssuesAsync();
        public abstract Task ImportIssuesAsync(string filePath);
        public abstract Task ExportIssuesAsync(string filePath);
    }
}
