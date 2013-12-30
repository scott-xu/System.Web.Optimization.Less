using System.IO;
using System.Web.Hosting;
using dotless.Core.Input;

namespace System.Web.Optimization
{
    /// <summary>
    ///     Extends <see cref="IFileReader" /> with features required for resolving virtual file
    ///     related to <see cref="ImportedFilePathResolver.CurrentDirectory" />.
    /// </summary>
    public interface IFileReaderWithResolver : IFileReader
    {
        ImportedFilePathResolver PathResolver { get; set; }
    }

    /// <summary>
    ///     Implements <see cref="IFileReader" /> with <see cref="IFileReaderWithResolver" /> extension required
    ///     for resolving virtual file related to <see cref="ImportedFilePathResolver.CurrentDirectory" />.
    /// </summary>
    public class LessVirtualFileReader : IFileReaderWithResolver
    {
        private ImportedFilePathResolver pathResolver;
        private VirtualFileReader virtualFileReader;

        public LessVirtualFileReader()
            : this(new ImportedFilePathResolver())
        {
        }

        protected LessVirtualFileReader(ImportedFilePathResolver pathResolver)
        {
            PathResolver = pathResolver;
        }

        public virtual bool DoesFileExist(string fileName)
        {
            fileName = pathResolver.GetFullPath(fileName);
            return HostingEnvironment.VirtualPathProvider.FileExists(fileName);
        }

        public virtual string GetFileContents(string fileName)
        {
            fileName = pathResolver.GetFullPath(fileName);
            using (Stream stream = HostingEnvironment.VirtualPathProvider.GetFile(fileName).Open())
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public virtual byte[] GetBinaryFileContents(string fileName)
        {
            fileName = pathResolver.GetFullPath(fileName);
            using (Stream stream = HostingEnvironment.VirtualPathProvider.GetFile(fileName).Open())
            {
                return GetBytes(stream);
            }
        }

        protected static byte[] GetBytes(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var ms = stream as MemoryStream;
            if (ms != null)
            {
                return ms.ToArray();
            }

            if (stream.CanSeek)
            {
                var content = new byte[stream.Length];
                stream.Read(content, 0, content.Length);
                return content;
            }

            var buffer = new byte[0x1000];
            using (ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public ImportedFilePathResolver PathResolver
        {
            get { return pathResolver; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                pathResolver = value;
            }
        }
    }
}
