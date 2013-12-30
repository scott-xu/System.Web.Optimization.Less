using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    ///     Stores LESS file dependencies. Takes into account transient files handled by <see cref="VirtualPathProvider" />.
    /// </summary>
    internal static class LessDependencyCache
    {
        /// <summary>
        ///     Lists of virtual paths of included files by the root less file cache key or virtual path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, IList<string>> _fileDependencies =
            new ConcurrentDictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        ///     Returns cache key for <paramref name="bundle" />.
        /// </summary>
        /// <param name="bundle"><see cref="Bundle" /> to get cache for.</param>
        /// <param name="context">Current <see cref="BundleContext" /></param>
        /// <returns>Cache key string.</returns>
        internal static string GetTransientBundleFilesKey(this Bundle bundle, BundleContext context)
        {
            return ComposeTransientFilesKey(bundle
                .EnumerateFiles(context)
                .SelectMany(file => new[] {file.IncludedVirtualPath}.Concat(
                    GetFileDependencies(bundle, file.IncludedVirtualPath, context))));
        }

        /// <summary>
        ///     Stores file dependency paths, if not saved yet.
        /// </summary>
        /// <param name="virtualPath">Root LESS file virtual path.</param>
        /// <param name="fileDependencies">Included file virtual paths.</param>
        internal static void SaveFileDependencies(string virtualPath, string[] fileDependencies)
        {
            string fileKey = HostingEnvironment.VirtualPathProvider.GetCacheKey(virtualPath) ?? virtualPath;
            _fileDependencies.TryAdd(fileKey, fileDependencies);

            string complexKey = GetTransientLessFileKey(virtualPath);
            if (complexKey != fileKey)
            {
                _fileDependencies.TryAdd(complexKey, fileDependencies);
            }
        }

        /// <summary>
        ///     Returns virtual paths of included files in <paramref name="virtualPath" /> LESS file, according to
        ///     <paramref name="context" />.
        ///     If not added yet, executes <see cref="LessTransform.Process" /> for specified <paramref name="bundle" /> and
        ///     ensures the dependencies are saved.
        /// </summary>
        /// <param name="bundle">Bundle to process, if not yet</param>
        /// <param name="virtualPath">Root LESS file to get dependencies for.</param>
        /// <param name="context">Current context.</param>
        /// <returns>Virtual paths of included files.</returns>
        private static IEnumerable<string> GetFileDependencies(Bundle bundle, string virtualPath, BundleContext context)
        {
            string key = HostingEnvironment.VirtualPathProvider.GetCacheKey(virtualPath) ?? virtualPath;

            Func<string, IList<string>> process = s =>
            {
                IEnumerable<BundleFile> files = bundle.EnumerateFiles(context);
                LessTransform.Process(ref files);
                return files.Select(file => file.IncludedVirtualPath).ToArray();
            };

            _fileDependencies.GetOrAdd(key, process);

            // returns more specific dependencies by the key containing transient file paths
            return _fileDependencies.GetOrAdd(GetTransientLessFileKey(virtualPath), process);
        }

        /// <summary>
        ///     Returns file cache key in respect to <see cref="VirtualPathProvider.GetCacheKey" /> results for all dependencies.
        /// </summary>
        /// <param name="virtualPath">Root LESS file virtual path.</param>
        /// <returns>File cache key.</returns>
        private static string GetTransientLessFileKey(string virtualPath)
        {
            string key = HostingEnvironment.VirtualPathProvider.GetCacheKey(virtualPath) ?? virtualPath;
            string dependencyKey = ComposeTransientFilesKey(_fileDependencies[key]);
            if (!string.IsNullOrEmpty(dependencyKey))
            {
                key += ";" + dependencyKey;
            }
            return key;
        }

        /// <summary>
        ///     Creates a cache key for provided virtual paths that <see cref="VirtualPathProvider.GetCacheKey" /> returns not
        ///     empty value for, otherwise empty string.
        /// </summary>
        /// <param name="paths">Virtual paths to pass to <see cref="VirtualPathProvider.GetCacheKey" /> and concat then.</param>
        /// <returns>Cache key or empty string.</returns>
        private static string ComposeTransientFilesKey(IEnumerable<string> paths)
        {
            VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
            return string.Join(";", paths
                .Distinct()
                .Select(vpp.GetCacheKey)
                .Where(k => !String.IsNullOrWhiteSpace(k))
                .ToArray());
        }
    }
}