using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auvo.ExcelConversor.Domain.Models
{
    public class ExcelLineModel
    {
        public int Codigo { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string ValorHora { get; set; }
        public DateTime Data { get; set; }
        public string HoraEntrada { get; set; } = string.Empty;
        public string HoraSaida { get; set; } = string.Empty;
        public string Almoco { get; set; } = string.Empty;
    }
}
