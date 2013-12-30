// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportedFilePathResolver.cs" company="Scott">
//   Copyright Scott Xu
// </copyright>
// <summary>
//   ImportedFile PathResolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System.Web.Optimization
{
    using dotless.Core.Input;

    /// <summary>
    /// The imported file path resolver.
    /// </summary>
    public class ImportedFilePathResolver : IPathResolver
    {
        /// <summary>
        /// The current file directory.
        /// </summary>
        private string currentDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedFilePathResolver"/> class.
        /// </summary>
        public ImportedFilePathResolver()
            : this(HttpRuntime.AppDomainAppVirtualPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedFilePathResolver"/> class.
        /// </summary>
        /// <param name="currentDirectory">
        /// The current app related directory path.
        /// </param>
        /// <exception cref="HttpException">
        /// <paramref name="currentDirectory"/> is not a valid virtual path
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="currentDirectory"/> is null
        /// </exception>
        public ImportedFilePathResolver(string currentDirectory)
        {
            this.CurrentDirectory = currentDirectory;
        }

        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        /// <exception cref="HttpException">
        /// <paramref name="value"/> is not a valid virtual path
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null
        /// </exception>
        public string CurrentDirectory
        {
            get
            {
                return this.currentDirectory;
            }

            set
            {
                this.currentDirectory = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAppRelative(value));
            }
        }

        /// <summary>
        /// Returns the absolute path for the specified imported file path.
        /// </summary>
        /// <param name="filePath">
        /// The imported file path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFullPath(string filePath)
        {
            if (filePath.StartsWith("/"))
            {
                filePath = VirtualPathUtility.ToAppRelative(filePath);
            }
            if (!filePath.StartsWith("~"))
            {
                filePath = VirtualPathUtility.Combine(this.currentDirectory, filePath);
            }

            return filePath;
        }
    }
}