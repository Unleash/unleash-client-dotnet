using System.Threading;
using System.Threading.Tasks;

namespace Unleash.Util
{
    internal interface IBackgroundTask
    {
        Task Execute(CancellationToken cancellationToken);
    }
}