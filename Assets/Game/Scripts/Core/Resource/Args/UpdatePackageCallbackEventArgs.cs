using Framework.Core.Events;

namespace Framework.Core
{
    public enum UpdatePackageCallbackType
    {
        再次初始化资源包,
        开始下载网络文件, // 此处直接使用抛出下载事件的形式，就不选用这个方式进行通知了
        再次更新静态版本,
        再次更新补丁清单,
        再次下载网络文件,
    }

    public class UpdatePackageCallbackEventArgs : CustomGameEventArgs
    {
        public static readonly int EventID = typeof(WebFileDownloadFailedEventArgs).GetHashCode();

        /// <summary>
        /// 被销毁敌人的ID
        /// </summary>
        public UpdatePackageCallbackType callbackType { get; set; }

        /// <summary>
        /// 构造函数（必须传入发送体和事件ID）
        /// </summary>
        public UpdatePackageCallbackEventArgs(object sender, int eventId) : base(sender, eventId)
        {
        }

        /// <summary>
        /// 快速创建事件实例（简化调用）
        /// </summary>
        public static UpdatePackageCallbackEventArgs Create(object sender, UpdatePackageCallbackType type)
        {
            return new UpdatePackageCallbackEventArgs(sender, EventID)
            {
                callbackType = type
            };
        }
    }
}