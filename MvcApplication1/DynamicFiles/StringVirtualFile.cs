using System.IO;
using System.Text;
using System.Web.Hosting;

namespace MvcApplication1.DynamicFiles
{
    /// <summary>
    ///     Simple plain text <see cref="VirtualFile" />.
    /// </summary>
    internal sealed class StringVirtualFile : VirtualFile
    {
        internal StringVirtualFile(string virtualPath, string content)
            : base(virtualPath)
        {
            Content = content ?? "";
        }

        public override Stream Open()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(Content));
        }

        public string Content { get; private set; }
    }
}