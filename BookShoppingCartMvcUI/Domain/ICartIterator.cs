namespace BookShoppingCartMvcUI.Domain
{
    public interface ICartIterator
    {
        ICartItem First();
        ICartItem Next();
        bool IsDone { get; }
        ICartItem CurrentItem { get; }
        // Bước nhảy khi lặp (ví dụ: Step = 2 để lấy mỗi phần tử thứ 2)
        int Step { get; set; }
    }
}
