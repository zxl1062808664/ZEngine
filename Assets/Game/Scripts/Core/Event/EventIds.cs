namespace Framework.Core.Events
{
    public partial class EventIds
    {
        // 1000-1100为框架事件ID
        // Resouce
        public const int UpdatePackageCallbackID = 1001;
        public const int InitializeFailedID = 1002;
        public const int PackageVersionUpdateFailedID = 1003;
        public const int PatchManifestUpdateFailedID = 1004;
        public const int WebFileDownloadFailedID = 1005;
        public const int UpdaterDoneID = 1006;
    }
}