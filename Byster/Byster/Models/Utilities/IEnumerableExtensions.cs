using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Byster.Models.Utilities
{
    public static class IEnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> ie)
        {
            if(ie == null) return null;
            ObservableCollection<T> collection = new ObservableCollection<T>();
            foreach(T item in ie)
            {
                collection.Add(item);
            }
            return collection;
        }
    }
}
