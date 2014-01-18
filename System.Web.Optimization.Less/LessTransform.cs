// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LessTransform.cs" company="Scott">
//   Copyright Scott Xu
// </copyright>
// <summary>
//   Defines the LessTransform type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using dotless.Core;
using dotless.Core.configuration;
using dotless.Core.Parser;

namespace System.Web.Optimization
{
    /// <summary>
    ///     The less transform.
    /// </summary>
    public class LessTransform : IBundleTransform
    {
        /// <summary>
        ///     The process.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="bundleResponse">
        ///     The bundle response.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Argument NULL Exception
        /// </exception>
        public void Process(BundleContext context, BundleResponse bundleResponse)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (bundleResponse == null)
            {
                throw new ArgumentNullException("bundleResponse");
            }

            context.HttpContext.Response.Cache.SetLastModifiedFromFileDependencies();

            IEnumerable<BundleFile> bundleFiles = bundleResponse.Files;
            bundleResponse.Content = Process(ref bundleFiles);
            // set bundle response files back (with imported ones)
            /*if (context.EnableOptimizations)
            {
                bundleResponse.Files = bundleFiles;
            }*/
            bundleResponse.ContentType = "text/css";
        }

        internal static string Process(ref IEnumerable<BundleFile> files)
        {
            DotlessConfiguration lessConfig = new WebConfigConfigurationLoader().GetConfiguration();

            if (!lessConfig.LessSource.GetInterfaces().Contains(typeof (IFileReaderWithResolver)))
            {
                lessConfig.LessSource = typeof (LessVirtualFileReader);
            }

            // system.Web.Optimization cache is used instead
            lessConfig.CacheEnabled = false;

            ILessEngine lessEngine = LessWeb.GetEngine(lessConfig);
            LessEngine underlyingLessEngine = lessEngine.ResolveLessEngine();
            Parser lessParser = underlyingLessEngine.Parser;
            var content = new StringBuilder();
            var urlRewrite = new CssRewriteUrlTransform();

            var targetFiles = new List<BundleFile>();

            foreach (BundleFile bundleFile in files)
            {
                targetFiles.Add(bundleFile);
                string filePath = bundleFile.IncludedVirtualPath;
                filePath = filePath.Replace('\\', '/');
                filePath = VirtualPathUtility.ToAppRelative(filePath);

                lessParser.SetCurrentFilePath(filePath);
                string source = bundleFile.ApplyTransforms();
                string extension = VirtualPathUtility.GetExtension(filePath);

                // if plain CSS file, do not transform LESS
                if (lessConfig.ImportAllFilesAsLess ||
                    ".less".Equals(extension, StringComparison.InvariantCultureIgnoreCase) ||
                    ".less.css".Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    string lessOutput = lessEngine.TransformToCss(source, filePath);

                    // pass the transformation result if successful
                    if (lessEngine.LastTransformationSuccessful)
                    {
                        source = lessOutput;
                    }
                    else
                    {
                        // otherwise write out error message.
                        // the transformation error is logged in LessEngine.TransformToCss
                        if (lessConfig.Debug)
                        {
                            content.AppendLine(string.Format(
                                "/* Error occurred in LESS transformation of the file: {0}. Please see details in the dotless log */",
                                bundleFile.IncludedVirtualPath));
                        }
                        continue;
                    }
                }

                source = urlRewrite.Process(bundleFile.IncludedVirtualPath, source);

                content.AppendLine(source);

                BundleFile[] fileDependencies = GetFileDependencies(underlyingLessEngine).ToArray();
                targetFiles.AddRange(fileDependencies);

                DependencyCache.SaveFileDependencies(filePath, fileDependencies.Select(file => file.IncludedVirtualPath).ToArray());
            }

            // include imports in bundle files to register cache dependencies
            files = BundleTable.EnableOptimizations ? targetFiles.Distinct() : targetFiles;

            return content.ToString();
        }

        /// <summary>
        ///     Gets the file dependencies (@imports) of the LESS file being parsed.
        /// </summary>
        /// <param name="lessEngine">The LESS engine.</param>
        /// <returns>An array of file references to the dependent file references.</returns>
        private static IEnumerable<BundleFile> GetFileDependencies(LessEngine lessEngine)
        {
            ImportedFilePathResolver pathResolver = lessEngine.Parser.GetPathResolver();
            VirtualPathProvider vpp = BundleTable.VirtualPathProvider;

            foreach (string resolvedVirtualPath in lessEngine.GetImports().Select(pathResolver.GetFullPath))
            {
                // the file was successfully imported, therefore no need to check before vpp.GetFile
                yield return new BundleFile(resolvedVirtualPath, vpp.GetFile(resolvedVirtualPath));
            }

            lessEngine.ResetImports();
        }
    }
}
