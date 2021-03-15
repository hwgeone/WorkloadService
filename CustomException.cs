using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkloadService.CalculateEngine
{
    public class CustomException:Exception
    {
        public string Type = "";
        public CustomException(string type)
        {
            Type = type;
        }
    }
}
