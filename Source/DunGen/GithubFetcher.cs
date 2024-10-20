#if FLAX_EDITOR
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DunGen;

/// <summary>
/// GithubFetcher Script.
/// </summary>
public class GithubFetcher
{
	private static readonly HttpClient httpClient = new HttpClient();

	public static async Task<string> FetchLatestReleaseAsync(string repoOwner, string repoName)
	{
		// Construct the GitHub API URL for the latest release
		string url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";

		// Set the User-Agent header (required by GitHub API)
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

		try
		{
			// Send a GET request to the URL
			HttpResponseMessage response = await httpClient.GetAsync(url);

			// Ensure the request was successful
			response.EnsureSuccessStatusCode();

			// Read the response content as a string
			string jsonResponse = await response.Content.ReadAsStringAsync();
			// FlaxEngine.Debug.Log("JSON Response: " + jsonResponse);

			// Optionally parse the JSON data
			JObject releaseData = JObject.Parse(jsonResponse);
			string tagName = releaseData["tag_name"]?.ToString();
			// string releaseNotes = releaseData["body"]?.ToString();
			return tagName;
			// FlaxEngine.Debug.Log($"Latest Release: {tagName}");
			// FlaxEngine.Debug.Log($"Release Notes: {releaseNotes}");
		}
		catch (HttpRequestException e)
		{
			FlaxEngine.Debug.Log($"Error fetching data: {e.Message}");
			return null;
		}
	}
}
#endif