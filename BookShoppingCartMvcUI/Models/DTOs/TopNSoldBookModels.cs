namespace BookShoppingCartMvcUI.Models.DTOs;

public class TopNSoldBookModel
{
    public string? BookName { get; set; }
    public string? AuthorName { get; set; }
    public string? Image { get; set; }
    public int TotalUnitSold { get; set; }
}

public record TopNSoldBooksVm(DateTime StartDate, DateTime EndDate, IEnumerable<TopNSoldBookModel> TopNSoldBooks);