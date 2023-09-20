using System;
using System.Runtime.Serialization;

namespace SampleFileMonitoring.Common.Exceptions
{
    public class MonitoringAgentException : ApplicationException
    {
        public MonitoringAgentException()
        {
        }

        public MonitoringAgentException(string message) : base(message)
        {
        }

        public MonitoringAgentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MonitoringAgentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
