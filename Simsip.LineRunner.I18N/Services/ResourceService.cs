using Simsip.LineRunner.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simsip.LineRunner.Services
{
    public class ResourceService
    {
        public static AppResources Resources;

        public AppResources LocalizedResources
        {
            get
            {
                return Resources ?? (Resources = new AppResources());
            }
        }
    }
}