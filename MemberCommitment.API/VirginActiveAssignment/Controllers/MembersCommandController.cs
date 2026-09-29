using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services;
using VirginActiveAssignment.Services.Exceptions;
using VirginActiveAssignment.Services.Factory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VirginActiveAssignment.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MembersCommandController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly string CacheKey = "RockMember";
        private readonly MemberService _memberService;
        private readonly ILogger<MembersCommandController> _logger;
        private readonly RockValidatorFactory _validatorFactory;
        public MembersCommandController(IMemoryCache cache, MemberService memberService, ILogger<MembersCommandController> logger, RockValidatorFactory validatorFactory)
        {
            _cache = cache;
            _memberService = memberService;
            _logger = logger;
            _validatorFactory = validatorFactory;
        }
        [HttpPost("{memberId}/rocks")]
        public async Task<ActionResult<RockModel>> CreateRock(int memberId, [FromBody] RockRequestModel request)
        {
            var member = await _memberService.GetProfileAsync(memberId);

            if(member==null)
            {
                throw new NotFoundException(string.Join(Environment.NewLine, "Member does not exist"));
            }


            foreach (var validator in _validatorFactory.GetValidators(request.Category))
            {
                validator.Validate(request);
            }

            //all pass now create rock

            var rock = new RockModel()
            {
                RockId = Guid.NewGuid(),
                MemberId = request.MemberId,
                Category = request.Category,
                Title = request.Title,
                DueDate = request.DueDate,
                Status = RockStatusEnum.Pending,
                Note = request.Note
            };
            var newKey = string.Join("_", [CacheKey, memberId]);

            var rocks = _cache.Get<List<RockModel>>(newKey)
                        ?? new List<RockModel>();

            rocks.Add(rock);

            _cache.Set(newKey, rocks);

            return CreatedAtAction(
                nameof(CreateRock),
                new { memberId, rockId = rock.RockId },
                rock);
        }

        [HttpPatch("{memberId}/rocks/{rockId}")]
        public async Task<ActionResult> UpdateRockStatus(int memberId, Guid rockId, [FromQuery] RockStatusEnum rockStatus)
        {

            var memberKey = string.Join("_", [CacheKey, memberId]);
            if (!_cache.TryGetValue(memberKey, out List<RockModel>? rockModels) || rockModels == null)
            {
                _logger.LogWarning("There are no Rocks for this Member {member}", memberId);
                throw new NotFoundException("There are no Rocks for this Member");
            }

            var rockModel = rockModels?.Where(x => x.RockId == rockId).FirstOrDefault();

            if (rockModel == null)
            {
                _logger.LogWarning("Rock {rockId} does not exist on this member {member}", rockId, memberId);
                throw new NotFoundException("This Rock does not exist on this member");

            }

            if(!RockStatusTransitions.IsValidTransition(rockModel.Status, rockStatus))
            {
                _logger.LogWarning("Cannot transition from {status} to {newStatus}", rockModel.Status, rockStatus);
                throw new InvalidStateTransitionException($"Cannot transition from {rockModel.Status} to {rockStatus}");
            }

            rockModel.Status = rockStatus;
            _cache.Set(memberKey, rockModels);

            return Ok();
        }
    }
}
