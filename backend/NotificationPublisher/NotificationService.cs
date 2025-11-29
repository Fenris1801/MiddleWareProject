using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class NotificationService : BackgroundService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly string _brokerUri;
    private IConnection? _connection;
    private ISession? _session;
    private readonly Random _rnd = new Random();

    private readonly string[] _topics = new[]
    {
        "weather",
        "pollution",
        "bikeAvailability",
        "traffic",
        "maintenance",
        "safety.alerts"
    };

    public NotificationService(ILogger<NotificationService> logger, string brokerUri)
    {
        _logger = logger;
        _brokerUri = brokerUri;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationService starting. BrokerUri={brokerUri}", _brokerUri);
        try
        {
            var factory = new ConnectionFactory(_brokerUri);
            _connection = factory.CreateConnection();
            _connection.Start();
            _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            _logger.LogInformation("Connected to ActiveMQ broker at {brokerUri}", _brokerUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to broker {brokerUri}", _brokerUri);
            throw;
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationService running. Publishing messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // choose random topic
                var topicName = _topics[_rnd.Next(_topics.Length)];
                var destination = _session!.GetTopic(topicName);
                using var producer = _session.CreateProducer(destination);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                var message = BuildRandomMessageForTopic(topicName);

                var text = JsonSerializer.Serialize(message);
                var textMessage = producer.CreateTextMessage(text);

                // set header with severity for convenience
                textMessage.Properties.SetString("topic", topicName);
                textMessage.Properties.SetString("severity", message.Severity);

                producer.Send(textMessage);

                _logger.LogInformation("Published to {topic}: severity={severity} payload={payload}",
                    topicName, message.Severity, text);

                // random wait between 0.5 and 3 seconds
                var delayMs = _rnd.Next(500, 3000);
                await Task.Delay(delayMs, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing notification. Will retry in 2s.");
                await Task.Delay(2000, stoppingToken);
            }
        }

        _logger.LogInformation("NotificationService stopping publish loop.");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationService stopping.");

        try
        {
            _session?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error when closing ActiveMQ connection.");
        }

        return base.StopAsync(cancellationToken);
    }

    private NotificationMessage BuildRandomMessageForTopic(string topic)
    {
        switch (topic)
        {
            case "weather":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "temperature", Math.Round(_rnd.NextDouble() * 35 - 5, 1) },
                        { "condition", RandomPick("clear","cloudy","rain","storm","snow") }
                    }
                };

            case "pollution":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "aqi", _rnd.Next(10, 301) },
                        { "main_pollutant", RandomPick("pm2.5","pm10","no2","o3") }
                    }
                };

            case "bikeAvailability":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "stationId", _rnd.Next(1, 200) },
                        { "availableBikes", _rnd.Next(0, 30) },
                        { "availableStands", _rnd.Next(0, 30) }
                    }
                };

            case "traffic":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "level", RandomPick("low","medium","high") },
                        { "description", RandomPick("roadworks","accident","heavy_traffic") }
                    }
                };

            case "maintenance":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = "info",
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "component", RandomPick("station_kiosk","bike_dock","payment_system") },
                        { "status", RandomPick("scheduled","in_progress","completed") }
                    }
                };

            case "safety.alerts":
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(highProbability: true),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "message", RandomPick("ride_with_care","road_closed","police_activity") }
                    }
                };

            default:
                return new NotificationMessage
                {
                    Topic = topic,
                    Severity = RandomSeverity(),
                    Timestamp = DateTime.UtcNow,
                    Payload = new Dictionary<string, object>
                    {
                        { "msg", "generic event" }
                    }
                };
        }
    }

    private string RandomSeverity(bool highProbability = false)
    {
        var list = highProbability ? new[] { "high", "high", "medium", "low" } : new[] { "low", "medium", "high" };
        return list[_rnd.Next(list.Length)];
    }

    private static string RandomPick(params string[] choices)
    {
        var r = new Random();
        return choices[r.Next(choices.Length)];
    }

    private class NotificationMessage
    {
        public string Topic { get; set; } = "";
        public string Severity { get; set; } = "info";
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Payload { get; set; } = new();
    }
}
