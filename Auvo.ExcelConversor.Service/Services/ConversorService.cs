using Auvo.ExcelConversor.Domain;
using Auvo.ExcelConversor.Domain.Models;
using Auvo.ExcelConversor.Service.Interfaces;
using Auvo.ExcelConversor.WebAPI.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auvo.ExcelConversor.Service.Services
{
    public class ConversorService : IConversorService
    {
        public ConversorService()
        {

        }

        #region ## PUBLIC METHODS
        public string ConversorExcelToJson(IFormFile file)
        {
            var fileLines = ReadFormFile(file);
            if (fileLines.Count <= 0)
                throw new Exception("Arquivo não possui dados a serem convertidos");

            var excelLinesModels = StringToExcelLineModel(fileLines);
            return JsonConvert.SerializeObject(ExcelLineToRoutineModel(excelLinesModels));
        }
        #endregion

        #region ## PRIVATE METHODS
        private List<string> ReadFormFile(IFormFile file)
        {
            var resultReading = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    resultReading.Add(reader.ReadLine());
                }
            }

            return resultReading;

        }

        private List<ExcelLineModel> StringToExcelLineModel(List<string> excelLines)
        {
            var result = new List<ExcelLineModel>();
            for (int i = 1; i < excelLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(excelLines[i]))
                {
                    var excelColumnsLine = excelLines[i].Split(";");
                    var excelLineModel = new ExcelLineModel
                    {
                        Codigo = int.Parse(excelColumnsLine[0]),
                        Nome = excelColumnsLine[1],
                        ValorHora = excelColumnsLine[2],
                        Data = DateTime.Parse(excelColumnsLine[3]),
                        HoraEntrada = excelColumnsLine[4],
                        HoraSaida = excelColumnsLine[5],
                        Almoco = excelColumnsLine[6],
                    };

                    result.Add(excelLineModel);
                }
            }
            return result;
        }

        private List<WorkRoutineModel> ExcelLineToRoutineModel(List<ExcelLineModel> listModel)
        {
            var listWorkRoutineModel = new List<WorkRoutineModel>();
            
            var anosExistentes = listModel.Select(x => x.Data.Year).Distinct().ToList();
            foreach (var ano in anosExistentes)
            {
                var mesesExistentes = listModel.Where(x => x.Data.Year == ano).Select(x => x.Data.Month).Distinct().ToList();
                foreach (var mes in mesesExistentes)
                {
                    var listModelMesVigencia = listModel.Where(x => x.Data.Year == ano && x.Data.Month == mes).ToList();
                    var workRoutineModel = new WorkRoutineModel();
                    workRoutineModel.Funcionarios = ExcelLineToWorkerModel(listModelMesVigencia);
                    workRoutineModel.TotalExtras = workRoutineModel.Funcionarios.Sum(x => x.HorasExtras);
                    workRoutineModel.TotalPagar = workRoutineModel.Funcionarios.Sum(x => x.TotalReceber);
                    workRoutineModel.TotalDescontos = workRoutineModel.Funcionarios.Sum(x => x.TotalDesconto);
                    workRoutineModel.MesVigencia = Util.GetMonthName(listModelMesVigencia[0].Data.Month);
                    workRoutineModel.AnoVigencia = listModelMesVigencia[0].Data.Year;
                    workRoutineModel.Departamento = "Campo não existente no documento";
                    listWorkRoutineModel.Add(workRoutineModel);
                }
            }


            return listWorkRoutineModel;
        }

        private List<WorkerModel> ExcelLineToWorkerModel(List<ExcelLineModel> listModel)
        {
            var workers = new List<WorkerModel>();

            for (int i = 0; i < listModel.Count; i++)
            {
                var line = listModel[i];
                var workerInList = workers.Where(x => x.Codigo == line.Codigo).ToList();
                if (workerInList.Count == 0)
                {
                    double horasTrabalhadas = 0.0;
                    var worker = new WorkerModel
                    {
                        Codigo = line.Codigo,
                        Nome = line.Nome,
                    };

                    var lineWorker = listModel.Where(x => x.Codigo == worker.Codigo).ToList();
                    worker.DiasTrabalhados = lineWorker.Count;

                    for (int j = 0; j < lineWorker.Count; j++)
                    {
                        var horasTrabalhadasDia = SomaHorasTrabalhadas(lineWorker[j].Almoco, lineWorker[j].HoraEntrada, lineWorker[j].HoraSaida);
                        horasTrabalhadas = horasTrabalhadas + horasTrabalhadasDia;
                        if ((horasTrabalhadasDia - 8) > 0)
                            worker.HorasExtras = worker.HorasExtras + (horasTrabalhadasDia - 8);
                        else if ((8 - horasTrabalhadasDia) > 0)
                            worker.HorasDebito = worker.HorasDebito + (8 - horasTrabalhadasDia);


                        worker.ValorHora = Util.TrataValorMonetario(lineWorker[j].ValorHora);
                        worker.TotalReceber = horasTrabalhadas * worker.ValorHora;
                        worker.TotalReceber = worker.TotalReceber + (worker.HorasExtras * worker.ValorHora);
                        worker.TotalDesconto = worker.TotalDesconto + (worker.HorasDebito * worker.ValorHora);
                    }

                    var diasDoMes = DateTime.DaysInMonth(lineWorker[0].Data.Year, lineWorker[0].Data.Month);
                    worker.DiasTrabalhados = lineWorker.Count;
                    var diasExtras = lineWorker.Count - diasDoMes;
                    if (diasExtras > 0)
                    {
                        worker.DiasExtras = diasExtras;
                        worker.TotalReceber = worker.TotalReceber + (diasExtras * worker.ValorHora);
                    }
                    else
                        worker.DiasExtras = 0;

                    var diasFalta = diasDoMes - lineWorker.Count;
                    if (diasFalta > 0)
                    {
                        worker.DiasFalta = diasFalta;
                        worker.TotalDesconto = worker.TotalDesconto + ((diasFalta * 8) * worker.ValorHora);
                    }
                    else
                        worker.DiasFalta = 0;

                    worker.TotalReceber = worker.TotalReceber - worker.TotalDesconto;
                    if (worker.TotalReceber < 0)
                        worker.TotalReceber = 0;

                    workers.Add(worker);
                }
            }

            return workers;
        }
        private double SomaHorasTrabalhadas(string almoco, string entrada, string saida)
        {
            var almocoSplit = almoco.Split('-');
            var entradaAlmoco = almocoSplit[0].ToString();
            var saidaAlmoco = almocoSplit[1].ToString();
            var horasDeAlmoco = TimeSpan.Parse(saidaAlmoco).Subtract(TimeSpan.Parse(entradaAlmoco));

            var horasDoDia = TimeSpan.Parse(saida).Subtract(TimeSpan.Parse(entrada));
            return horasDoDia.Subtract(horasDeAlmoco).TotalHours;
        }


        #endregion
    }
}
