using System.Threading.Tasks;

namespace PCL.Core.IO.Download.Scheduler;

public interface IDownloadScheduler
{
    void Schedule(DownloadTask task);
}