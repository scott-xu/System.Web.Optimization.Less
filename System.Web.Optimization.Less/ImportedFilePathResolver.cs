// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportedFilePathResolver.cs" company="Scott">
//   Copyright Scott Xu
// </copyright>
// <summary>
//   ImportedFile PathResolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using dotless.Core.Input;

namespace System.Web.Optimization
{
    /// <summary>
    ///     The imported file path resolver.
    /// </summary>
    public class ImportedFilePathResolver : IPathResolver
    {
        /// <summary>
        ///     The current file directory.
        /// </summary>
        private string _currentDirectory;

        /// <summary>
        ///     Sets the path to directory of the currently processed file.
        /// </summary>
        /// <exception cref="HttpException">
        ///     <paramref name="value" /> is not a valid virtual path
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null
        /// </exception>
        public string CurrentDirectory
        {
            get
            {
                if (_currentDirectory != null)
                {
                    return _currentDirectory;
                }
                HttpContext ctx = HttpContext.Current;
                return ctx != null ? VirtualPathUtility.GetDirectory(ctx.Request.AppRelativeCurrentExecutionFilePath) : HttpRuntime.AppDomainAppVirtualPath;
            }
            set { _currentDirectory = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAppRelative(value)); }
        }

        /// <summary>
        ///     Returns the absolute path for the specified imported file path.
        /// </summary>
        /// <param name="filePath">
        ///     The imported file path.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetFullPath(string filePath)
        {
            if (filePath.StartsWith("/"))
            {
                filePath = VirtualPathUtility.ToAppRelative(filePath);
            }
            if (!filePath.StartsWith("~"))
            {
                filePath = VirtualPathUtility.Combine(CurrentDirectory, filePath);
            }

            return filePath;
        }
    }
}
