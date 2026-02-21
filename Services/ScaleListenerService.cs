using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PortListener.Services;

namespace PortListener.Services;

public class ScaleListenerService : BackgroundService
{
    private readonly IWeightStorageService _weightStorage;
    private readonly ILogger<ScaleListenerService> _logger;

    // Only accept data from this sender IP and port
    private readonly IPAddress _allowedSenderIP = IPAddress.Parse("192.168.0.55");
    private readonly int _port = 4321;
    
    private const string JsonFilePath = "data/scale_data.json";
    private readonly object _fileLock = new object();

    public ScaleListenerService(
        IWeightStorageService weightStorage,
        ILogger<ScaleListenerService> logger)
    {
        _weightStorage = weightStorage;
        _logger = logger;
        
        // Ensure data directory exists
        Directory.CreateDirectory("data");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TcpClient? client = null;
            try
            {
                client = new TcpClient();
                _logger.LogInformation("Connecting to scale at {IP}:{Port} (TCP)...", _allowedSenderIP, _port);
                
                // Try to connect with a timeout
                var connectTask = client.ConnectAsync(_allowedSenderIP, _port, stoppingToken).AsTask();
                if (await Task.WhenAny(connectTask, Task.Delay(5000, stoppingToken)) != connectTask)
                {
                    throw new Exception("Connection timeout");
                }
                
                _logger.LogInformation("✅ Connected to scale at {IP}:{Port}", _allowedSenderIP, _port);

                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (!stoppingToken.IsCancellationRequested && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                    if (bytesRead == 0)
                    {
                        _logger.LogWarning("Connection closed by the scale.");
                        break;
                    }

                    string text = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    _logger.LogInformation("Received content: \"{Content}\"", text);

                    // Extract weight value from message (format: "1.282KG")
                    string? weight = ExtractWeight(text);
                    
                    if (weight == null)
                    {
                        _logger.LogWarning("Could not extract weight from message: \"{Message}\"", text);
                    }

                    // Create data object
                    var dataEntry = new WeightData
                    {
                        Timestamp = DateTime.Now,
                        Message = text,
                        Weight = weight != null && double.TryParse(weight, out double w) ? w : null,
                        WeightUnit = "kg"
                    };

                    // Update in-memory storage
                    _weightStorage.UpdateWeight(dataEntry);

                    // Save to JSON file (JSONL format)
                    string json = JsonSerializer.Serialize(dataEntry);
                    lock (_fileLock)
                    {
                        File.AppendAllText(JsonFilePath, json + Environment.NewLine);
                    }

                    _logger.LogInformation("✅ Processed Message: {Message}", text);
                    if (weight != null)
                    {
                        _logger.LogInformation("⚖️  Extracted Weight: {Weight} kg", weight);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Connection error: {Message}. Retrying in 5 seconds...", ex.Message);
                client?.Close();
                await Task.Delay(5000, stoppingToken);
            }
            finally
            {
                client?.Close();
            }
        }
    }

    private static string? ExtractWeight(string message)
    {
        // Support format like "RTW:0.650 kg", "1.282KG", or just "0.200"
        var match = Regex.Match(message, @"(?:RTW:)?([\d.]+)(?:\s*(?:kg|KG))?", RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }
        return null;
    }
}
