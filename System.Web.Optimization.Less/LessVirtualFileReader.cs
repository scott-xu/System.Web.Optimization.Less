using System.IO;
using dotless.Core.Input;

namespace System.Web.Optimization
{
    /// <summary>
    ///     Extends <see cref="IFileReader" /> with features required for resolving virtual file
    ///     related to the current directory set with <see cref="ImportedFilePathResolver.SetCurrentDirectory" />.
    /// </summary>
    public interface IFileReaderWithResolver : IFileReader
    {
        ImportedFilePathResolver PathResolver { get; set; }
    }

    /// <summary>
    ///     Implements <see cref="IFileReader" /> with <see cref="IFileReaderWithResolver" /> extension required
    ///     for resolving virtual file related to the current directory set with
    ///     <see cref="ImportedFilePathResolver.SetCurrentDirectory" />.
    /// </summary>
    public class LessVirtualFileReader : IFileReaderWithResolver
    {
        private ImportedFilePathResolver _pathResolver;

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
            fileName = _pathResolver.GetFullPath(fileName);
            return BundleTable.VirtualPathProvider.FileExists(fileName);
        }

        public virtual string GetFileContents(string fileName)
        {
            fileName = _pathResolver.GetFullPath(fileName);
            using (Stream stream = BundleTable.VirtualPathProvider.GetFile(fileName).Open())
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public virtual byte[] GetBinaryFileContents(string fileName)
        {
            fileName = _pathResolver.GetFullPath(fileName);
            using (Stream stream = BundleTable.VirtualPathProvider.GetFile(fileName).Open())
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

        public bool UseCacheDependencies
        {
            get { return false; }
        }

        public ImportedFilePathResolver PathResolver
        {
            get { return _pathResolver; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _pathResolver = value;
            }
        }
    }
}
