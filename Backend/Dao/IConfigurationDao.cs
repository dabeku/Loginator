using Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Dao {
    public interface IConfigurationDao {
        Configuration Read();
        void Write(Configuration configuration);
    }
}
