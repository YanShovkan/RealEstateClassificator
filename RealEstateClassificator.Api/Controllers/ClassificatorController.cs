﻿using Microsoft.AspNetCore.Mvc;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificatorController : ControllerBase
    {
        private readonly IPageParserService _pageParserService;
        private readonly ICardParserService _cardParserService;
        public ClassificatorController(IPageParserService pageParserService, ICardParserService cardParserService)
        {
            _pageParserService = pageParserService;
            _cardParserService = cardParserService;
        }

        [HttpGet("parse")]
        public IEnumerable<Card> StartParsing()
        {
            var caca = new List<Card>();

            foreach(var cards in _pageParserService.GetCardsFromNextPage())
            {
                _cardParserService.ParseRealEstates(cards);
            }

            return caca;
        }
    }
}
