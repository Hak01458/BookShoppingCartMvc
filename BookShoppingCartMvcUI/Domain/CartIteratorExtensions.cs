using System.Collections.Generic;

namespace BookShoppingCartMvcUI.Domain
{
    public static class CartIteratorExtensions
    {
        public static IEnumerable<ICartItem> AsEnumerable(this ICartIterator iterator)
        {
            for (ICartItem item = iterator.First(); !iterator.IsDone; item = iterator.Next())
            {
                if (item != null) yield return item;
            }
        }
    }
}
