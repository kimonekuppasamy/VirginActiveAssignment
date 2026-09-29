using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MemberCommitment.API.Tests.Integration
{
    /// <summary>
    /// 5. The valid key must not be hardcoded anywhere in the code.
    /// </summary>
    public class ApiKeyAuthenticationTests
    {
        [Fact]
        public void ApiKeyValue_IsNotHardcodedInSource()
        {
            var projectDirectory = ApiProjectDirectory();
            var apiKey = ReadAppSettingsApiKey(projectDirectory);
            var sourceFiles = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToList();

            Assert.NotEmpty(sourceFiles);
            Assert.All(sourceFiles, file => Assert.DoesNotContain(apiKey, File.ReadAllText(file)));
        }

        private static string ReadAppSettingsApiKey(string projectDirectory)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(projectDirectory, "appsettings.json")));
            return document.RootElement.GetProperty("ApiKey").GetString()!;
        }

        private static string ApiProjectDirectory([CallerFilePath] string thisFile = "")
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(thisFile)!);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "VirginActiveAssignment", "VirginActiveAssignment.csproj");
                if (File.Exists(candidate))
                {
                    return Path.GetDirectoryName(candidate)!;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate the API project directory.");
        }
    }
}
