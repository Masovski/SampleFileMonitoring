using Newtonsoft.Json;
using SampleFileMonitoring.Common;
using SampleFileMonitoring.Services.DTO;

namespace SampleFileMonitoring.Services
{
    internal static class Extensions
    {
        internal static SystemDetailsDTO AsDTO(this SystemDetails systemDetails)
        {
            return new()
            {
                LogonUsername = systemDetails.LogonUsername,
                Domain = systemDetails.Domain,
                IPv4Address = systemDetails.IPv4Addresses,
                OperatingSystemName = systemDetails.OperatingSystemName
            };
        }

        internal static FileDetailsDTO AsDTO(this FileDetails fileDetails)
        {
            return new()
            {
                FileName = fileDetails.FileName,
                FileType = fileDetails.FileType,
                Location = fileDetails.Location,
                Hash = fileDetails.Hash,
                CreatedAt = fileDetails.CreatedAtUtc,
                ModifiedDate = fileDetails.ModifiedDateUtc,
                FileSize = fileDetails.FileSize
            };
        }

        internal static string AsJSON(this SystemDetails systemDetails)
        {
            return JsonConvert.SerializeObject(systemDetails.AsDTO());
        }

        internal static string AsJSON(this FileDetails fileDetails)
        {
            return JsonConvert.SerializeObject(fileDetails.AsDTO());
        }
    }
}
