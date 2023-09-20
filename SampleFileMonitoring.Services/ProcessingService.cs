using Microsoft.Extensions.Logging;
using SampleFileMonitoring.Common;
using SampleFileMonitoring.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SampleFileMonitoring.Services
{
    public class ProcessingService : IProcessingService
    {
        private readonly ILogger<ProcessingService> _logger;
        private readonly ISystemService _systemService;
        private readonly HttpClient _httpClient;

        private readonly string _registerFileEndpoint;
        private readonly string _registerSystemEndpoint;

        public ProcessingService(
            ILogger<ProcessingService> logger,
            ISystemService systemService,
            HttpClient httpClient,
            ProcessingEndpointSettings endpointSettings)
        {
            this._logger = logger;
            _systemService = systemService;
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri(endpointSettings.BaseAddress);
            _registerFileEndpoint = endpointSettings.RegisterFileEndpointPath;
            _registerSystemEndpoint = endpointSettings.RegisterSystemEndpointPath;
        }

        public async Task RegisterFileAsync(string path)
        {
            var fileInfo = await _systemService.GetFileDetailsAsync(path);
            _logger.LogInformation($"Registering {fileInfo.FileName}...");

            var jsonBody = new StringContent(fileInfo.AsJSON(), Encoding.UTF8, "application/json");
            await ProcessPostRequest(() => _httpClient.PostAsync(_registerFileEndpoint, jsonBody));
        }

        public async Task RegisterSystemAsync()
        {
            var systemInfo = await _systemService.GetSystemDetailsAsync();
            _logger.LogInformation($"Registering System with OS: {systemInfo.OperatingSystemName}");

            var jsonBody = new StringContent(systemInfo.AsJSON(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_registerSystemEndpoint, jsonBody);
            await ProcessPostRequest(() => _httpClient.PostAsync(_registerSystemEndpoint, jsonBody));
        }

        private async Task ProcessPostRequest(Func<Task<HttpResponseMessage>> requestTask)
        {
            try
            {
                var response = await requestTask();
                await LogHttpResponse(response); // Just logging the response since this is only for demo purposes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task LogHttpResponse(HttpResponseMessage response) 
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug(responseContent);
                return;
            }

            _logger.LogError($"POST request failed with status code: {response.StatusCode}");
            _logger.LogError($"Error Response Content: {responseContent}");
        }
    }
}
