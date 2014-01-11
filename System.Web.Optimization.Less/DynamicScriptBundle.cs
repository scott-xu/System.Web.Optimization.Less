using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// The Script bundle with support of transient files implemented with a custom <see cref="VirtualPathProvider"/>.
    /// </summary>
    public class DynamicScriptBundle : DynamicBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicScriptBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        public DynamicScriptBundle(string virtualPath)
            : this(virtualPath, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicScriptBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        /// <param name="cdnPath">
        /// The CDN path.
        /// </param>
        public DynamicScriptBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath, new JsMinify())
        {
        }
    }
}