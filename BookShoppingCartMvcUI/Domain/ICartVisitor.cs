namespace BookShoppingCartMvcUI.Domain
{
    public interface ICartVisitor
    {
        void Visit(BookLeaf leaf);
        void Visit(BundleComposite composite);
    }
}
