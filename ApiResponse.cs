using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaConnect2
{
    public class ApiResponse<T>
    {
        public int messageCode { get; set; }
        public string messageString { get; set; }
        public T data { get; set; }
    }
}
