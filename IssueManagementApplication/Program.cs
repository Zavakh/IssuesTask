using IssueManagementLibrary.Managers;
using IssueManagementLibrary.Models;
using IssueManagerLibrary;

namespace IssueManagerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Choose a service: 1 for GitHub, 2 for GitLab");
            var choice = Console.ReadLine();

            IssueManager manager = null;
            if (choice == "1")
            {
                Console.WriteLine("Enter GitHub repo owner:");
                var repoOwner = Console.ReadLine();
                Console.WriteLine("Enter GitHub repo name:");
                var repoName = Console.ReadLine();
                Console.WriteLine("Enter GitHub token:");
                var token = Console.ReadLine();
                manager = new GitHubIssueManager(repoOwner, repoName, token);
            }
            else if (choice == "2")
            {
                Console.WriteLine("Enter GitLab project ID:");
                var projectId = Console.ReadLine();
                Console.WriteLine("Enter GitLab token:");
                var token = Console.ReadLine();
                manager = new GitLabIssueManager(projectId, token);
            }

            if (manager == null)
            {
                Console.WriteLine("Invalid choice");
                return;
            }

            while (true)
            {
                try
                {
                    Console.WriteLine("1. Add Issue");
                    Console.WriteLine("2. Update Issue");
                    Console.WriteLine("3. Close Issue");
                    Console.WriteLine("4. Export Issues");
                    Console.WriteLine("5. Import Issues");
                    Console.WriteLine("6. Exit");
                    var option = Console.ReadLine();
                    var response = ResponseEnum.OK;

                    switch (option)
                    {
                        case "1":
                            Console.WriteLine("Enter issue title:");
                            var title = Console.ReadLine();
                            Console.WriteLine("Enter issue description:");
                            var body = Console.ReadLine();
                            response = await manager.AddIssueAsync(new IssueModel { Title = title, Description = body });
                            if (response == ResponseEnum.UNAUTHORIZED)
                            {
                                Console.WriteLine("You do not have rights to perform this action.");
                            } 
                            break;
                        case "2":
                            Console.WriteLine("Enter issue ID to update:");
                            var updateId = int.Parse(Console.ReadLine());
                            Console.WriteLine("Enter new issue title:");
                            var newTitle = Console.ReadLine();
                            Console.WriteLine("Enter new issue description:");
                            var newBody = Console.ReadLine();
                            response = await manager.UpdateIssueAsync(updateId, new IssueModel { Title = newTitle, Description = newBody });
                            if (response == ResponseEnum.UNAUTHORIZED)
                            {
                                Console.WriteLine("You do not have rights to perform this action.");
                            }
                            else if (response == ResponseEnum.NOT_FOUND)
                            {
                                Console.WriteLine("No issue with given ID was found.");
                            }
                            break;
                        case "3":
                            Console.WriteLine("Enter issue ID to close:");
                            var closeId = int.Parse(Console.ReadLine());
                            response = await manager.CloseIssueAsync(closeId);
                            if (response == ResponseEnum.UNAUTHORIZED)
                            {
                                Console.WriteLine("You do not have rights to perform this action.");
                            }
                            else if (response == ResponseEnum.NOT_FOUND)
                            {
                                Console.WriteLine("No issue with given ID was found.");
                            }
                            break;
                        case "4":
                            Console.WriteLine("Enter file path to export issues:");
                            var exportPath = Console.ReadLine();
                            await manager.ExportIssuesAsync(exportPath);
                            break;
                        case "5":
                            Console.WriteLine("Enter file path to import issues:");
                            var importPath = Console.ReadLine();
                            await manager.ImportIssuesAsync(importPath);
                            if (response == ResponseEnum.NODATA)
                            {
                                Console.WriteLine("No data found.");
                            }
                            break;
                        case "6":
                            return;
                        default:
                            Console.WriteLine("Invalid option");
                            break;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                }                
            }
        }
    }
}
