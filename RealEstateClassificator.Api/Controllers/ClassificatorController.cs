using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificatorController : ControllerBase
    {
        private readonly ICrawler _crawler;
        public ClassificatorController(ICrawler crawler)
        {
            _crawler = crawler;
        }

        [HttpGet("parse")]
        public async Task<IEnumerable<Card>> StartParsing()
        {
            var caca = new List<Card>();

            await foreach(var cards in _crawler.GetCardsFromNextPageOrUrls())
            {
                caca.AddRange(cards);
            }

            return caca;
        }
    }
}
