using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRGardenAlpha.Data;
using VRGardenAlpha.Filters;
using VRGardenAlpha.Models;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/trades")]
    public class TradeController : ControllerBase
    {
        private readonly GardenContext _ctx;
        private readonly IMapper _mapper;

        public TradeController(GardenContext ctx, IMapper mapper)
        {
            _ctx = ctx;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> GetStatsAsync()
        {
            var count = await _ctx.Trades.CountAsync();
            var active = await _ctx.Trades.CountAsync(x => x.Initiator.Agreed && x.Recepient.Agreed);

            return Ok(new
            {
                count,
                active
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTradeAsync(string id)
        {
            var trade = await _ctx.Trades
                .ProjectTo<TradeModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            return Ok(trade);
        }

        [HttpPost]
        [RequiresCaptcha]
        [ProducesResponseType(200, Type = typeof(TradeModel))]
        public async Task<IActionResult> CreateTradeAsync([FromBody] CreateTradeModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var trade = new Trade()
            {
                ACL = ACL.Incomplete,
                Initiator = new()
                {
                    Title = request.Title,
                    Trader = request.Trader,
                    Creator = request.Creator,
                    Passcode = request.Passcode,
                    ContentLink = request.ContentLink,
                    ContentLength = -1,
                    ImageContentType = "image/jpeg",
                    ContentType = "application/octet-stream",
                    Checksum = "PENDING_FILE_UPLOAD",
                    FileName = "PENDING_FILE_UPLOAD",
                },
                Recepient = new()
                {
                    Title = request.Recipient.Title,
                    Trader = request.Recipient.Trader,
                    Creator = request.Recipient.Creator,
                    ContentLink = request.Recipient.ContentLink,
                    ContentLength = -1,
                    ImageContentType = "image/jpeg",
                    ContentType = "application/octet-stream",
                    Checksum = "PENDING_FILE_UPLOAD",
                    FileName = "PENDING_FILE_UPLOAD",
                }
            };

            await _ctx.Trades.AddAsync(trade);
            await _ctx.SaveChangesAsync();

            return Ok(_mapper.Map<TradeModel>(trade));
        }

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadTradeImageAsync()
        {

            return Ok();
        }

        [HttpPost("{id}/file")]
        public async Task<IActionResult> UploadTradeFileAsync()
        {

            return Ok();
        }
    }
}
