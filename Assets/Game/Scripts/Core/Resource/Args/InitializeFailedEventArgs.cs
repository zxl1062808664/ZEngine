using Framework.Core.Events;

namespace Framework.Core
{
    public class InitializeFailedEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public InitializeFailedEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static InitializeFailedEventArgs Create(object sender)
        {
            return new InitializeFailedEventArgs(sender, EventID)
            {
            };
        }
    }

    public class PackageVersionUpdateFailedEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public PackageVersionUpdateFailedEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static PackageVersionUpdateFailedEventArgs Create(object sender)
        {
            return new PackageVersionUpdateFailedEventArgs(sender, EventID)
            {
            };
        }
    }

    public class PatchManifestUpdateFailedEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public PatchManifestUpdateFailedEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static PatchManifestUpdateFailedEventArgs Create(object sender)
        {
            return new PatchManifestUpdateFailedEventArgs(sender, EventID)
            {
            };
        }
    }

    public class FoundUpdateFilesEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(FoundUpdateFilesEventArgs).GetHashCode();

        public int TotalCount { get; set; }
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public FoundUpdateFilesEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static FoundUpdateFilesEventArgs Create(object sender, int totalCount, long totalSizeBytes)
        {
            return new FoundUpdateFilesEventArgs(sender, EventID)
            {
                TotalCount = totalCount,
                TotalSizeBytes = totalSizeBytes
            };
        }
    }

    public class WebFileDownloadFailedEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        public string FileName { get; set; }
        public string Error { get; set; }

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public WebFileDownloadFailedEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static WebFileDownloadFailedEventArgs Create(object sender, string fileName, string error)
        {
            return new WebFileDownloadFailedEventArgs(sender, EventID)
            {
                FileName = fileName,
                Error = error
            };
        }
    }

    public class DownloadProgressUpdateEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(DownloadProgressUpdateEventArgs).GetHashCode();

        public int TotalDownloadCount { get; set; }
        public int CurrentDownloadCount { get; set; }
        public long TotalDownloadSizeBytes { get; set; }
        public long CurrentDownloadSizeBytes { get; set; }

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public DownloadProgressUpdateEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static DownloadProgressUpdateEventArgs Create(object sender, int totalDownloadCount,
            int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
        {
            return new DownloadProgressUpdateEventArgs(sender, EventID)
            {
                TotalDownloadCount = totalDownloadCount,
                CurrentDownloadCount = currentDownloadCount,
                TotalDownloadSizeBytes = totalDownloadSizeBytes,
                CurrentDownloadSizeBytes = currentDownloadSizeBytes
            };
        }
    }

    public class UpdaterDoneEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public UpdaterDoneEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static UpdaterDoneEventArgs Create(object sender)
        {
            return new UpdaterDoneEventArgs(sender, EventID)
            {
            };
        }
    }

    public class InitializeAssetsFinishEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(InitializeAssetsFinishEventArgs).GetHashCode();
        public bool isFinished { get; set; }

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public InitializeAssetsFinishEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static InitializeAssetsFinishEventArgs Create(object sender, bool isFinished)
        {
            return new InitializeAssetsFinishEventArgs(sender, EventID)
            {
                isFinished = isFinished
            };
        }
    }
}