using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Model {

    public class Property {
        public string Name { get; set; }
        public string Value { get; set; }

        public Property(string name, string value) {
            Name = name;
            Value = value;
        }
    }
}
