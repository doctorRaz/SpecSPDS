using drz.Abstractions.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.LogServices.drzNlog
{
    public sealed class NLogService_ : IDrzLogService
    {

        public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

        public IDrzLogger GetLogger(Type type)
        {
            //if (type == null) throw new ArgumentNullException(nameof(type));
            //string productName = SafeGetProductName();
            //LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);
            //return new NLogAdapter(factory.GetLogger(type.FullName)); // <-- оборачиваем
            throw new NotSupportedException("Не доделано");
        }
    }
}