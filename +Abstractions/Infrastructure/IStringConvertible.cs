using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>Предоставляет методы для строкового представления объекта.</summary>
    public interface IStringConvertible
    {
        /// <summary>Converts to longstring.</summary>
        /// <returns>long string</returns>
        string ToLongString();

        /// <summary>Converts to shortstring.</summary>
        /// <returns>short string</returns>
        string ToShortString();

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        string ToString();
    }
}
