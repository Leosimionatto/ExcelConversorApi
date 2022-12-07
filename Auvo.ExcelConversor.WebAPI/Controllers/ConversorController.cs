using Auvo.ExcelConversor.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auvo.ExcelConversor.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversorController : ControllerBase
    {
        private readonly IConversorService _conversionService;

        public ConversorController(IConversorService conversionService)
        {
            _conversionService = conversionService;
        }

        /// <summary>
        /// Conversor de excel para JSON
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("conversor-excel-to-json")]
        public string ConversorExcelToJson(IFormFile file)
        {
            return _conversionService.ConversorExcelToJson(file);
        }

        /// <summary>
        /// Conversor de excel para JSON (DOWNLOAD)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("conversor-excel-to-json/download")]
        public IActionResult ConversorExcelToJsonDownload(IFormFile file)
        {
            var result = _conversionService.ConversorExcelToJson(file);
            byte[] byteArray = System.Text.ASCIIEncoding.ASCII.GetBytes(result);
            return File(byteArray, "application/force-download", "resultfile.json");
        }
    }
}