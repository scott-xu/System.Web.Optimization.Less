// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LessBundle.cs" company="Scott">
//   Copyright Scott Xu
// </copyright>
// <summary>
//   Defines the LessBundle type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// The less bundle.
    /// </summary>
    public class LessBundle : StyleBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        public LessBundle(string virtualPath)
            : this(virtualPath, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        /// <param name="cdnPath">
        /// The CDN path.
        /// </param>
        public LessBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath)
        {
            Transforms.Insert(0, new LessTransform());
        }

        /// <summary>
        /// Gets the cache key for the specified bundle context. Takes into account transient files handled by <see cref="VirtualPathProvider"/>.
        /// </summary>
        /// <param name="context"><see cref="BundleContext"/> instance.</param>
        /// <returns>Cache key string.</returns>
        public override string GetCacheKey(BundleContext context)
        {
            var key = base.GetCacheKey(context);
            var filesKey = this.GetTransientBundleFilesKey(context);

            if (!string.IsNullOrEmpty(filesKey))
            {
                key += ":" + filesKey;
            }
            return key;
        }
    }
}