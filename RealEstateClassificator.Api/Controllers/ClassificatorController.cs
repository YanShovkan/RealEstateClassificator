using Microsoft.AspNetCore.Mvc;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificatorController : ControllerBase
    {
        private readonly IPageParserService _pageParserService;
        public ClassificatorController(IPageParserService pageParserService)
        {
            _pageParserService = pageParserService;
        }

        [HttpGet("parse")]
        public IEnumerable<Card> StartParsing()
        {
            var caca = new List<Card>();

            foreach(var cards in _pageParserService.GetCardsFromNextPage())
            {
                Console.WriteLine("ПАША ИГОШИН ГЕЙ!");
            }

            return caca;
        }
    }
}
