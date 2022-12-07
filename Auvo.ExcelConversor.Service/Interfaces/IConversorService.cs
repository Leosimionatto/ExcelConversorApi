using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auvo.ExcelConversor.Service.Interfaces
{
    public interface IConversorService
    {
        public string ConversorExcelToJson(IFormFile file);
    }
}
