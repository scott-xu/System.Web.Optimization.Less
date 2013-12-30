// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LessTransform.cs" company="Scott">
//   Copyright Scott Xu
// </copyright>
// <summary>
//   Defines the LessTransform type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System.Web.Optimization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Hosting;
    using dotless.Core;
    using dotless.Core.configuration;
    using dotless.Core.Parser;

    /// <summary>
    /// The less transform.
    /// </summary>
    public class LessTransform : IBundleTransform
    {
        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="bundleResponse">
        /// The bundle response.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Argument NULL Exception
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
            bundleResponse.Files = bundleFiles;
            bundleResponse.ContentType = "text/css";
        }

        internal static string Process(ref IEnumerable<BundleFile> files)
        {
            var lessConfig = new WebConfigConfigurationLoader().GetConfiguration();
            var lessEngine = LessWeb.GetEngine(lessConfig);
            var underlyingLessEngine = lessEngine.ResolveLessEngine();
            var lessParser = underlyingLessEngine.Parser;
            var content = new StringBuilder();

            var targetFiles = new List<BundleFile>();

            foreach (var bundleFile in files)
            {
                targetFiles.Add(bundleFile);
                var filePath = bundleFile.IncludedVirtualPath;
                filePath = filePath.Replace('\\', '/');
                filePath = VirtualPathUtility.ToAppRelative(filePath);

                lessParser.SetCurrentFilePath(filePath);
                var source = bundleFile.ApplyTransforms();
                var extension = VirtualPathUtility.GetExtension(filePath);

                // if plain CSS file, do not transform LESS
                if (lessConfig.ImportAllFilesAsLess ||
                    ".less".Equals(extension, StringComparison.InvariantCultureIgnoreCase) ||
                    ".less.css".Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var lessOutput = lessEngine.TransformToCss(source, filePath);

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

                content.AppendLine(source);

                var fileDependencies = GetFileDependencies(lessParser, bundleFile.VirtualFile).ToArray();
                targetFiles.AddRange(fileDependencies);

                LessDependencyCache.SaveFileDependencies(filePath, fileDependencies.Select(file => file.IncludedVirtualPath).ToArray());
            }

            // include imports in bundle files to register cache dependencies
            files = BundleTable.EnableOptimizations ? targetFiles.Distinct() : targetFiles;

            return content.ToString();
        }

        /// <summary>
        /// Gets the file dependencies (@imports) of the LESS file being parsed.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        /// <param name="virtualFile">The virtual file</param>
        /// <returns>An array of file references to the dependent file references.</returns>
        private static IEnumerable<BundleFile> GetFileDependencies(Parser lessParser, VirtualFile virtualFile)
        {
            var pathResolver = lessParser.GetPathResolver();

            foreach (var importPath in lessParser.Importer.Imports)
            {
                yield return new BundleFile(pathResolver.GetFullPath(importPath), virtualFile);
            }

            lessParser.Importer.Imports.Clear();
        }
    }
}
