using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Shared
{
    public interface IReportGenerator
    {
        Task<string> GenerateAsync(DateTime startDate, DateTime endDate);
    }

    public abstract class ReportGeneratorBase<TModel> : IReportGenerator
    {
        // Template method
        public async Task<string> GenerateAsync(DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);
            var data = await FetchDataAsync(startDate, endDate);
            var output = Format(data);
            await PostProcessAsync(data);
            return output;
        }

        // Hook/primitive operations
        protected virtual void ValidateDates(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("endDate must be greater than or equal to startDate");
        }

        protected abstract Task<IEnumerable<TModel>> FetchDataAsync(DateTime startDate, DateTime endDate);

        protected abstract string Format(IEnumerable<TModel> data);

        // Optional hook
        protected virtual Task PostProcessAsync(IEnumerable<TModel> data) => Task.CompletedTask;
    }
}
