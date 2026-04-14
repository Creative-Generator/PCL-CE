namespace PCL.Core.IO.Download;

public enum ChunkEventType
{
    Started,
    Progress,
    Completed,
    Failed,
    Paused
}