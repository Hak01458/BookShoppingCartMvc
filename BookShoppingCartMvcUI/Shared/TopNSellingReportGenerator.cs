using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BookShoppingCartMvcUI.Repositories;
using BookShoppingCartMvcUI.Models.DTOs;

namespace BookShoppingCartMvcUI.Shared
{
    public class TopNSellingReportGenerator : ReportGeneratorBase<TopNSoldBookModel>
    {
        private readonly IReportRepository _reportRepo;

        public TopNSellingReportGenerator(IReportRepository reportRepo)
        {
            _reportRepo = reportRepo ?? throw new ArgumentNullException(nameof(reportRepo));
        }

        protected override async Task<IEnumerable<TopNSoldBookModel>> FetchDataAsync(DateTime startDate, DateTime endDate)
        {
            return await _reportRepo.GetTopNSellingBooksByDate(startDate, endDate);
        }

        protected override string Format(IEnumerable<TopNSoldBookModel> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BookName,AuthorName,TotalUnitSold");
            foreach (var item in data)
            {
                var name = EscapeForCsv(item.BookName);
                var author = EscapeForCsv(item.AuthorName);
                sb.AppendLine($"\"{name}\",\"{author}\",{item.TotalUnitSold}");
            }
            return sb.ToString();
        }

        private static string EscapeForCsv(string? s) => (s ?? string.Empty).Replace("\"", "\"\"");
    }
}
