using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminEventsApiClient
    {
        Task<EventsResult> GetEvents(CancellationToken cancellationToken = default(CancellationToken));
    }
}