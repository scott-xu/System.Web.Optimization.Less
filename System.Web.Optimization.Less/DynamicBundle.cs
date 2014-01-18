using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    ///     Extends <see cref="Bundle" /> with dynamic cache key functionality.
    /// </summary>
    public class DynamicBundle : Bundle
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="DynamicBundle" />
        /// </summary>
        /// <param name="virtualPath">Virtual path of the bundle</param>
        public DynamicBundle(string virtualPath)
            : this(virtualPath, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="DynamicBundle" />
        /// </summary>
        /// <param name="virtualPath">Virtual path of the bundle</param>
        /// <param name="cdnPath">CDN path of the bundle</param>
        public DynamicBundle(string virtualPath, string cdnPath)
            : this(virtualPath, cdnPath, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="DynamicBundle" />
        /// </summary>
        /// <param name="virtualPath">Virtual path of the bundle</param>
        /// <param name="cdnPath">CDN path of the bundle</param>
        /// <param name="transforms">Bundle transforms</param>
        public DynamicBundle(string virtualPath, string cdnPath, params IBundleTransform[] transforms)
            : base(virtualPath, cdnPath, transforms)
        {
        }

        /// <summary>
        ///     Gets the cache key for the specified bundle context. Takes into account transient files handled by
        ///     <see cref="VirtualPathProvider" />.
        /// </summary>
        /// <param name="context"><see cref="BundleContext" /> instance.</param>
        /// <returns>Cache key string.</returns>
        public override string GetCacheKey(BundleContext context)
        {
            string key = base.GetCacheKey(context);
            string filesKey = this.GetTransientBundleFilesKey(context);

            if (!string.IsNullOrEmpty(filesKey))
            {
                key += ":" + filesKey;
            }
            return key;
        }
    }
}
