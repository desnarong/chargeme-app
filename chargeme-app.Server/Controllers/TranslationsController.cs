using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranslationsController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;

        public TranslationsController(NpgsqlDbContext context)
        {
            _context = context;
        }

        [HttpGet("{languageCode}")]
        public IActionResult GetTranslations(string languageCode)
        {
            var translations = from t in _context.TblLanguageData
                               join l in _context.TblLanguages on t.FLanguageId equals l.FId
                               where l.FCode == languageCode
                               select new
                               {
                                   t.FName,
                                   t.FText,
                                   t.FValue
                               };

            return Ok(translations.ToDictionary(t => t.FText, t => t.FValue));
        }
        [HttpGet("languages")]
        public IActionResult GetLanguages()
        {
            var translations = from t in _context.TblLanguages
                               select new
                               {
                                   t.FName,
                                   t.FCode,
                               };

            return Ok(translations.ToDictionary(t => t.FCode, t => t.FName));
        }
    }
}
