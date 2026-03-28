using System.Collections.Generic;
using System.Linq;

namespace BookShoppingCartMvcUI.Domain
{
    public class BundleComposite : ICartItem
    {
        private readonly List<ICartItem> _children = new();

        public BundleComposite(string name, int quantity = 1)
        {
            Name = name;
            Quantity = quantity;
        }

        public string Name { get; }
        public decimal Price => _children.Sum(c => c.Price); // price per bundle (you may decide different rules)
        public int Quantity { get; set; }

        public IReadOnlyCollection<ICartItem> Children => _children.AsReadOnly();

        public void AddChild(ICartItem item) => _children.Add(item);
        public void RemoveChild(ICartItem item) => _children.Remove(item);

        public decimal GetTotalPrice()
        {
            // total = (sum child totals) * bundle quantity
            var sum = _children.Sum(c => c.GetTotalPrice());
            return sum * Quantity;
        }

        public ICartItem Clone()
        {
            var copy = new BundleComposite(Name, Quantity);
            // deep clone children
            foreach (var child in _children)
            {
                copy.AddChild(child.Clone());
            }
            return copy;
        }

        public ICartIterator CreateIterator()
        {
            return new CartIterator(_children);
        }

        public void Accept(ICartVisitor visitor)
        {
            visitor.Visit(this);
            // traverse children
            foreach (var child in _children)
            {
                child.Accept(visitor);
            }
        }
    }
}
