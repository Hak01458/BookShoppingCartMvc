namespace BookShoppingCartMvcUI.Domain
{
    public interface ICartItem
    {
        string Name { get; }
        decimal Price { get; }
        int Quantity { get; set; }

        // Total price for this node (leaf or composite)
        decimal GetTotalPrice();

        // Prototype: create a deep copy of this cart item
        ICartItem Clone();

        // Optional: children for composite nodes
        IReadOnlyCollection<ICartItem> Children { get; }
        void AddChild(ICartItem item);   // composite: add; leaf: may throw
        void RemoveChild(ICartItem item);

        // Iterator factory: create an iterator for this cart item
        ICartIterator CreateIterator();
    }
}
