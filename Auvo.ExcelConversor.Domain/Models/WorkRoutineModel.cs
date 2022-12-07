using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auvo.ExcelConversor.Domain.Models
{

    public class WorkRoutineModel
    {
        public string Departamento { get; set; } = string.Empty;
        public string MesVigencia { get; set; } = string.Empty;
        public int AnoVigencia { get; set; }
        public double TotalPagar { get; set; }
        public double TotalDescontos { get; set; }
        public double TotalExtras { get; set; }
        public List<WorkerModel> Funcionarios { get; set; }
    }

   
}
