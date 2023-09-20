using System;
using System.Collections.Generic;
using System.Text;

namespace SampleFileMonitoring.Common
{
    public class ProcessingEndpointSettings
    {
        public string BaseAddress { get; set; }
        public string RegisterFileEndpointPath { get; set; }
        public string RegisterSystemEndpointPath { get; set; }
    }
}
