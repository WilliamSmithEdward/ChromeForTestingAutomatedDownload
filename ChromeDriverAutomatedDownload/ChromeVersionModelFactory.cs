﻿using System.Text.Json;

namespace ChromeForTestingAutomatedDownload
{
    public static class ChromeVersionModelFactory
    {
        public static async Task<T> CreateChromeVersionModel<T>() where T : IChromeVersionModel, new()
        {
            var response = await new T().QueryEndpoint();

            var deserializedObject = JsonSerializer.Deserialize<T>(response);
            if (deserializedObject != null) return deserializedObject;

            throw new JsonException("Failed to deserialize endpoint.");
        }
    }
}