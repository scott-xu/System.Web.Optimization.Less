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
    using System.IO;
    using System.Web.Hosting;

    using dotless.Core.Input;

    /// <summary>
    /// The imported file path resolver.
    /// </summary>
    public class ImportedFilePathResolver : IPathResolver
    {
        /// <summary>
        /// The current file directory.
        /// </summary>
        private string currentFileDirectory;

        /// <summary>
        /// The current file path.
        /// </summary>
        private string currentFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedFilePathResolver"/> class.
        /// </summary>
        /// <param name="currentFilePath">
        /// The current file path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// current file path should not be null nor empty
        /// </exception>
        public ImportedFilePathResolver(string currentFilePath)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                throw new ArgumentNullException("currentFilePath");
            }

            this.CurrentFilePath = currentFilePath;
        }

        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        public string CurrentFilePath
        {
            get
            {
                return this.currentFilePath;
            }

            set
            {
                this.currentFilePath = value;
                this.currentFileDirectory = Path.GetDirectoryName(value);
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
            if (filePath.StartsWith("~"))
            {
                filePath = VirtualPathUtility.ToAbsolute(filePath);
            }

            if (filePath.StartsWith("/"))
            {
                filePath = HostingEnvironment.MapPath(filePath);
            }
            else if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.GetFullPath(Path.Combine(this.currentFileDirectory, filePath));
            }

            return filePath;
        }
    }
}
