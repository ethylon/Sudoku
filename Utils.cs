using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public static class Utils
    {
        public static void ForAll<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (T item in @this)
                action(item);
        }
    }
}
