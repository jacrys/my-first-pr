using Microsoft.AspNetCore.Mvc.RazorPages;

namespace my_first_pr.Pages
{
    public class IndexModel : PageModel
    {
        public IReadOnlyList<Issue> baseIssues { get; set; }
        public IReadOnlyList<ModIssue> Issues { get; set; }
        public IReadOnlyList<Repository> Repositories { get; set; }
        public IReadOnlyList<Repository> StarredRepos { get; set; }
        public IReadOnlyList<User> Followers { get; set; }
        public IReadOnlyList<User> Following { get; set; }
        public Octokit.Language Langs { get; set; }

        public static readonly SearchIssuesRequest HacktoberfestRequest = new SearchIssuesRequest() {
            State = ItemState.Open,
            Labels = new List<string>() {
                "hacktoberfest"
            }
        };

        public static readonly SearchIssuesRequest HacktoberfestFirstRequest = new SearchIssuesRequest() {
            State = ItemState.Open,
            Labels = new List<string>() {
                "hacktoberfest",
                "good-first-issue"
            }
        };

        public static readonly SearchIssuesRequest HacktoberfestEasyRequest = new SearchIssuesRequest() {
            State = ItemState.Open,
            Labels = new List<string>() {
                "hacktoberfest",
                "difficulty:+easy"
            }
        };

        public async Task OnGetAsyc() {}

        public async Task OnPostAsync(string language,string search)
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                string accessToken = await HttpContext.GetTokenAsync("access_token");
                var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"), new InMemoryCredentialStore(new Credentials(accessToken)));
                // Repositories = await github.Repository.GetAllForCurrent();
                // StarredRepos = await github.Activity.Starring.GetAllForCurrent();
                // Followers = await github.User.Followers.GetAllForCurrent();
                // Following = await github.User.Followers.GetAllFollowingForCurrent();
                var currentSearch = GetSearchIssuesRequest(search);
                if (!String.IsNullOrEmpty(language))
                {
                    currentSearch.Language = (Octokit.Language)(Int32.Parse(language));
                }
                else
                {
                    currentSearch.Language = null;
                }
                baseIssues = (await github.Search.SearchIssues(currentSearch)).Items;
                currentSearch = null;
                Issues = (IReadOnlyList<ModIssue>)baseIssues.Select(x => new ModIssue(x)).ToList();
                foreach (var issue in Issues)
                {
                    string response = (string)((await github.Connection.GetRaw( new Uri(issue.Url), null) ).HttpResponse.Body);
                    ModIssue iss = JsonConvert.DeserializeObject<ModIssue>(response);
                    issue.RepositoryUrl = iss.RepositoryUrl;
                    response = (string)( await github.Connection.GetRaw( new Uri( issue.RepositoryUrl ), null ) ).HttpResponse.Body;
                    JObject repo = JObject.Parse(response);
                    JObject owner = (JObject)repo["owner"];
                    var own = new User((string)owner["avatar_url"], null, null, 0, null, default(DateTimeOffset), default(DateTimeOffset), 0, null, 0, 0, null, null, 0, 0, null, (string)owner["login"], null, null, 0, null, 0, 0, 0, null, null, false, null, null);
                    issue.Repository = new Repository(null, (string)repo["html_url"], null, null, null, null, null, 0, null, own, null, null, false, null, null, (string)repo["language"], false, false, 0, 0, null, 0, null, default(DateTimeOffset), default(DateTimeOffset), null, null, null, null, false, false, false, false, 0, 0, null, null, null, false, 0, null, default(RepositoryVisibility));
                }
            }
        }

        private SearchIssuesRequest GetSearchIssuesRequest(string search)
		{
            switch (search)
			{
                case "HACKTOBERFEST_ISSUES":
                    return HacktoberfestRequest;
                case "HACKTOBERFEST_EASY_ISSUES":
                    return HacktoberfestEasyRequest;
                case "HACKTOBERFEST_FIRST_ISSUES":
                    return HacktoberfestFirstRequest;
                default:
                    return null;
            }
		}
    }

    public class ModIssue : Issue
	{
        public string RepositoryUrl { get; set; }
        new public Repository Repository { get; set; }
        public string LanguageIcon
        {
            get
            {
                switch (Repository.Language)
                {
                    case "C":
                        return "<i class=\"nf lang-icon nf-custom-c\"></i>";
                    case "C++":
                        return "<i class=\"nf lang-icon nf-custom-cpp\"></i>";
                    case "C#":
                        return "<i class=\"nf lang-icon nf-mdi-language_csharp\"></i>";
                    case "Elixir":
                        return "<i class=\"nf lang-icon nf-custom-elixir\"></i>";
                    case "Go":
                        return "<i class=\"nf lang-icon nf-seti-go\"></i>";
                    case "HTML":
                        return "<i class=\"nf lang-icon nf-dev-html5\"></i>";
                    case "Java":
                        return "<i class=\"nf lang-icon nf-dev-java px-1\"></i>";
                    case "JavaScript":
                        return "<i class=\"nf lang-icon nf-dev-javascript_badge\"></i>";
                    case "PHP":
                        return "<i class=\"nf lang-icon nf-mdi-language_php\"></i>";
                    case "Python":
                        return "<i class=\"nf lang-icon nf-dev-python\"></i>";
                    case "Ruby":
                        return "<i class=\"nf lang-icon nf-dev-ruby\"></i>";
                    case "Scala":
                        return "<i class=\"nf lang-icon nf-dev-scala\"></i>";
                    case "TypeScript":
                        return "<i class=\"nf lang-icon nf-mdi-language_typescript\"></i>";
                    case "Rust":
                        return "<i class=\"nf lang-icon nf-dev-rust\"></i>";
                    default:
                        return $"<span>{Repository.Language}</span>";
                }
            }
        }

        [JsonConstructor]
        public ModIssue (string url, string html_url, string comments_url, string events_url, int number, ItemState state, string title, string body, User closed_by, User user, IReadOnlyList<Label> labels, User assignee, IReadOnlyList<User> assignees, Milestone milestone, int comments, PullRequest pullRequest, DateTimeOffset? closed_at, DateTimeOffset created_at, DateTimeOffset? updated_at, int id, string node_id, bool locked, Repository repository, string repository_url, ReactionSummary reactions)
        {
            Id = id;
            NodeId = node_id;
            Url = url;
            HtmlUrl = html_url;
            CommentsUrl = comments_url;
            EventsUrl = events_url;
            Number = number;
            State = state;
            Title = title;
            Body = body;
            ClosedBy = closed_by;
            User = user;
            Labels = labels;
            Assignee = assignee;
            Assignees = assignees;
            Milestone = milestone;
            Comments = comments;
            PullRequest = pullRequest;
            ClosedAt = closed_at;
            CreatedAt = created_at;
            UpdatedAt = updated_at;
            Locked = locked;
            Repository = repository;
            RepositoryUrl = repository_url;
            Reactions = reactions;
        }

        public ModIssue (Issue issue)
		{
                Id = issue.Id;
                NodeId = issue.NodeId;
                Url = issue.Url;
                HtmlUrl = issue.HtmlUrl;
                CommentsUrl = issue.CommentsUrl;
                EventsUrl = issue.EventsUrl;
                Number = issue.Number;
                State = issue.State;
                Title = issue.Title;
                Body = issue.Body;
                ClosedBy = issue.ClosedBy;
                User = issue.User;
                Labels = issue.Labels;
                Assignee = issue.Assignee;
                Assignees = issue.Assignees;
                Milestone = issue.Milestone;
                Comments = issue.Comments;
                PullRequest = issue.PullRequest;
                ClosedAt = issue.ClosedAt;
                CreatedAt = issue.CreatedAt;
                UpdatedAt = issue.UpdatedAt;
                Locked = issue.Locked;
                Repository = issue.Repository;
                Reactions = issue.Reactions;
		}
	}

    public class ModSearchIssuesResult : SearchResult<ModIssue>
    {
        public ModSearchIssuesResult() { }

        public ModSearchIssuesResult(int totalCount, bool incompleteResults, IReadOnlyList<ModIssue> items)
            : base(totalCount, incompleteResults, items)
        {
        }
    }

}
