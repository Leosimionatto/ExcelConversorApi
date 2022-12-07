using System.Globalization;

namespace Auvo.ExcelConversor.WebAPI.Util
{
    public static class Util
    {
        public static double TrataValorMonetario(string valorHora)
        {
            valorHora = valorHora.Replace("R$", "");
            valorHora = valorHora.Replace(" ", "");

            return double.Parse(valorHora);
        }
        public static string GetMonthName(int month)
        {
            var culture = new CultureInfo("pt-BR");
            DateTimeFormatInfo dtfi = culture.DateTimeFormat;
            return culture.TextInfo.ToTitleCase(dtfi.GetMonthName(month));
        }
    }
}
