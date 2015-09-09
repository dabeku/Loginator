using Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Converter {
    public interface ILogConverter {
        IEnumerable<Log> Convert(string text);
    }
}
