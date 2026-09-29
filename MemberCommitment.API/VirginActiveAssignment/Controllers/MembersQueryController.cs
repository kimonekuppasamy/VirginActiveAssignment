using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services;

namespace VirginActiveAssignment.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MembersQueryController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly string CacheKey = "RockMember";
        private readonly MemberService _memberService;
        private readonly ILogger<MembersQueryController> _logger;
        public MembersQueryController(IMemoryCache cache, MemberService memberService, ILogger<MembersQueryController> logger)
        {
            _cache = cache;
            _memberService = memberService;
            _logger = logger;
        }
        [HttpGet("{memberId}/rocks")]
        public async Task<ActionResult<List<RockModel>?>> GetRocksForMember(int memberId, [FromQuery] RockStatusEnum? rockStatus = null)
        {
            var memberKey = string.Join("_", [CacheKey, memberId]);
            _cache.TryGetValue(memberKey, out List<RockModel>? rockModels);
            if (rockModels != null && rockStatus != null)
            {
                var filtered = rockModels.Where(x => x.Status == rockStatus).ToList();
                return filtered;
            }
            return rockModels;
        }

        [HttpGet("{memberId}/profile/enriched")]
        public async Task<ActionResult<EnrichedMemberModel>> GetEnrichedMemberProfile(int memberId, CancellationToken cancellationToken = default)
        {

            var memberKey = string.Join("_", [CacheKey, memberId]);

            _cache.TryGetValue(
                memberKey,
                out List<RockModel>? rocks);

            rocks ??= [];

            MemberModel? profile = null;

            try
            {
                profile = await _memberService.GetProfileAsync(
                    memberId,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Profile enrichment unavailable for member {MemberId}",
                    memberId);

            }

            return Ok(new EnrichedMemberModel
            {
                Member = profile,
                Rocks = rocks,
                EnrichmentDataAvailable = profile != null
            });

        }
    }
}
