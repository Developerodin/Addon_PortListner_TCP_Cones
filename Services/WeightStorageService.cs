namespace PortListener.Services;

public class WeightData
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string WeightUnit { get; set; } = "kg";
}

public interface IWeightStorageService
{
    WeightData? GetLatestWeight();
    void UpdateWeight(WeightData data);
}

public class WeightStorageService : IWeightStorageService
{
    private WeightData? _latestWeight;
    private readonly object _lock = new object();

    public WeightData? GetLatestWeight()
    {
        lock (_lock)
        {
            return _latestWeight;
        }
    }

    public void UpdateWeight(WeightData data)
    {
        lock (_lock)
        {
            _latestWeight = data;
        }
    }
}
