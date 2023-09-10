using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services
{
    public class ServiceProvider
    {
        public static Microsoft.Extensions.DependencyInjection.ServiceProvider Current { get; set; }
    }
}
