using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avantis.Settings
{
    public static class SBOTables
    {
        public static List<Type> GetTables()
        {
            List<Type> _tables = new List<Type>();

            _tables.Add(typeof(Category));
            _tables.Add(typeof(Trademark));
            _tables.Add(typeof(Model));

            return _tables;
        }
    }
}
