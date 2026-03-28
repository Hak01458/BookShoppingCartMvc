using System.Collections.Generic;

namespace BookShoppingCartMvcUI.Domain
{
    public class CartIterator : ICartIterator
    {
        private readonly IList<ICartItem> _items;
        private int _current = 0;
        private int _step = 1;

        public CartIterator(IList<ICartItem> items)
        {
            _items = items ?? new List<ICartItem>();
        }

        public ICartItem First()
        {
            _current = 0;
            return _items.Count > 0 ? _items[_current] : null;
        }

        public ICartItem Next()
        {
            _current += _step;
            return !IsDone ? _items[_current] : null;
        }

        public bool IsDone => _current >= _items.Count;

        public ICartItem CurrentItem => !IsDone ? _items[_current] : null;

        public int Step { get => _step; set => _step = value < 1 ? 1 : value; }
    }
}
