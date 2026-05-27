using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ModelContextProtocol.Tests.Server;

/// <summary>
/// Tests for task status notification functionality in McpServer.
/// </summary>
public class McpServerTaskNotificationTests : ClientServerTestBase
{
    public McpServerTaskNotificationTests()
        : base()
    {
    }

    [Test]
    public async Task NotifyTaskStatusAsync_SendsNotificationWithTaskDetails()
    {
        // Arrange
        var client = await CreateMcpClientForServer();
        var tcs = new TaskCompletionSource<McpTaskStatusNotificationParams>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        await using var registration = client.RegisterNotificationHandler(
            NotificationMethods.TaskStatusNotification,
            (notification, cancellationToken) =>
            {
                if (notification.Params is { } paramsNode)
                {
                    var notificationParams = JsonSerializer.Deserialize<McpTaskStatusNotificationParams>(paramsNode, McpJsonUtilities.DefaultOptions);
                    if (notificationParams is not null)
                    {
                        tcs.TrySetResult(notificationParams);
                    }
                }
                return default;
            });

        var mcpTask = new McpTask
        {
            TaskId = "task-123",
            Status = McpTaskStatus.Working,
            StatusMessage = "Processing request",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            LastUpdatedAt = DateTimeOffset.UtcNow,
            TimeToLive = TimeSpan.FromMinutes(10),
            PollInterval = TimeSpan.FromSeconds(1)
        };

        // Act
        await Server.NotifyTaskStatusAsync(mcpTask, TestContext.CurrentContext.CancellationToken);
        var notification = await tcs.Task.WaitAsync(TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Equal(mcpTask.TaskId, notification.TaskId);
        Assert.Equal(mcpTask.Status, notification.Status);
        Assert.Equal(mcpTask.StatusMessage, notification.StatusMessage);
        Assert.Equal(mcpTask.CreatedAt, notification.CreatedAt);
        Assert.Equal(mcpTask.LastUpdatedAt, notification.LastUpdatedAt);
        Assert.Equal(mcpTask.TimeToLive, notification.TimeToLive);
        Assert.Equal(mcpTask.PollInterval, notification.PollInterval);
    }

    [Test]
    public async Task NotifyTaskStatusAsync_ThrowsOnNullTask()
    {
        // Arrange
        await CreateMcpClientForServer();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Server.NotifyTaskStatusAsync(null!, TestContext.CurrentContext.CancellationToken));
    }

    [Test]
    public async Task NotifyTaskStatusAsync_SendsMultipleNotificationsForDifferentStatuses()
    {
        // Arrange
        var client = await CreateMcpClientForServer();
        var receivedNotifications = new ConcurrentBag<McpTaskStatusNotificationParams>();
        int expectedCount = 3;
        var allReceivedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        await using var registration = client.RegisterNotificationHandler(
            NotificationMethods.TaskStatusNotification,
            (notification, cancellationToken) =>
            {
                if (notification.Params is { } paramsNode)
                {
                    var notificationParams = JsonSerializer.Deserialize<McpTaskStatusNotificationParams>(paramsNode, McpJsonUtilities.DefaultOptions);
                    if (notificationParams is not null)
                    {
                        receivedNotifications.Add(notificationParams);
                        if (receivedNotifications.Count >= expectedCount)
                        {
                            allReceivedTcs.TrySetResult(true);
                        }
                    }
                }
                return default;
            });

        // Act - Send notifications for different statuses
        var task1 = new McpTask
        {
            TaskId = "task-456",
            Status = McpTaskStatus.Working,
            StatusMessage = "Starting",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            LastUpdatedAt = DateTimeOffset.UtcNow,
            TimeToLive = TimeSpan.FromMinutes(10),
            PollInterval = TimeSpan.FromSeconds(1)
        };
        
        var task2 = new McpTask
        {
            TaskId = "task-456",
            Status = McpTaskStatus.Working,
            StatusMessage = "Processing",
            CreatedAt = task1.CreatedAt,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            TimeToLive = TimeSpan.FromMinutes(10),
            PollInterval = TimeSpan.FromSeconds(1)
        };
        
        var task3 = new McpTask
        {
            TaskId = "task-456",
            Status = McpTaskStatus.Completed,
            StatusMessage = "Done",
            CreatedAt = task1.CreatedAt,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            TimeToLive = TimeSpan.FromMinutes(10),
            PollInterval = TimeSpan.FromSeconds(1)
        };

        await Server.NotifyTaskStatusAsync(task1, TestContext.CurrentContext.CancellationToken);
        await Server.NotifyTaskStatusAsync(task2, TestContext.CurrentContext.CancellationToken);
        await Server.NotifyTaskStatusAsync(task3, TestContext.CurrentContext.CancellationToken);
        
        // Wait for all notifications to be received
        await allReceivedTcs.Task.WaitAsync(TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Equal(3, receivedNotifications.Count);
        Assert.Contains(receivedNotifications, n => n.Status == McpTaskStatus.Working && n.StatusMessage == "Starting");
        Assert.Contains(receivedNotifications, n => n.Status == McpTaskStatus.Working && n.StatusMessage == "Processing");
        Assert.Contains(receivedNotifications, n => n.Status == McpTaskStatus.Completed && n.StatusMessage == "Done");
    }
}
