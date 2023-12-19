using Microsoft.AspNetCore.Mvc;
using RealEstateClassificator.Core.Services.Interfaces;

namespace RealEstateClassificator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificatorController : ControllerBase
    {
        private readonly IPageParserService _pageParserService;
        private readonly ICardParserService _cardParserService;
        private readonly IClassificationService _classificationService;
        public ClassificatorController(IPageParserService pageParserService, ICardParserService cardParserService, IClassificationService classificationService)
        {
            _pageParserService = pageParserService;
            _cardParserService = cardParserService;
            _classificationService = classificationService;
        }

        [HttpGet("parse")]
        public async Task StartParsing()
        {
            foreach (var cards in _pageParserService.GetCardsFromNextPage())
            {
                await _cardParserService.ParseRealEstatesAsync(cards);
            }
        }

        [HttpGet("classificate")]
        public async Task StartClassificating()
        {
             await _classificationService.CalculateRealEstateClass();
        }
    }
}
