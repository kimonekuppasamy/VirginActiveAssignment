using System.Net;
using VirginActiveAssignment.Models;

namespace VirginActiveAssignment.Services
{
    public class MemberService
    {
        private readonly HttpClient _httpClient;

        public MemberService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MemberModel?> GetProfileAsync(
            int memberId,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                $"users/{memberId}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MemberModel>(cancellationToken);
        }
    }
}
