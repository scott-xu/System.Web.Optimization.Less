using System.IO;
using System.Web.Hosting;

namespace MvcApplication1.BootstrapTheme
{
    /// <summary>
    ///     Simple <see cref="HostingEnvironment.MapPath" /> based implementation of <see cref="VirtualFile" />.
    /// </summary>
    internal sealed class VirtualPathFile : VirtualFile
    {
        internal VirtualPathFile(string virtualPath)
            : base(virtualPath)
        {
        }

        public override Stream Open()
        {
            return File.Open(HostingEnvironment.MapPath(VirtualPath), FileMode.Open, FileAccess.Read);
        }
    }
}
