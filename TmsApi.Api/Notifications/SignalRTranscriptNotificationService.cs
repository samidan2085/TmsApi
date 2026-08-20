using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Application.Hubs;
namespace TmsApi.Api.Notifications;

public class SignalRTranscriptNotificationService(IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ITranscriptNotificationService
{
    public async Task NotifyTranscriptReadyAsync(int studentId, string reportId, string downloadUrl)
    {
        await hubContext.Clients
            .Group(GroupNames.Student(studentId.ToString()))
            .ReceiveTranscriptReady(reportId, downloadUrl);
    }
}