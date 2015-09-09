using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions {
    public class LoginatorException : ApplicationException {
        public LoginatorException(string message) : base(message) { }
        public LoginatorException(string message, Exception e) : base(message, e) { }
    }
}
