using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Parser;

namespace System.Web.Optimization
{
    /// <summary>
    ///     Specifies dotless related utilities.
    /// </summary>
    internal static class LessExtensions
    {
        /// <summary>
        ///     Informs the LESS parser about the path to the currently processed file.
        ///     This is done by using a custom <see cref="IPathResolver" /> implementation.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        /// <param name="currentFilePath">The path to the currently processed file.</param>
        internal static void SetCurrentFilePath(this Parser lessParser, string currentFilePath)
        {
            ImportedFilePathResolver pathResolver = GetPathResolver(lessParser);
            pathResolver.CurrentDirectory = VirtualPathUtility.GetDirectory(currentFilePath);
            lessParser.FileName = currentFilePath;
        }

        /// <summary>
        ///     Returns an <see cref="IPathResolver" /> instance used by the specified LESS lessParser.
        /// </summary>
        /// <param name="lessParser">
        ///     The LESS parser.
        /// </param>
        /// <returns>
        ///     The <see cref="IPathResolver" />.
        /// </returns>
        internal static ImportedFilePathResolver GetPathResolver(this Parser lessParser)
        {
            IFileReaderWithResolver fileReader = GetFileReaderWithResolver(lessParser);
            if (fileReader.PathResolver == null)
            {
                throw new InvalidOperationException("Unexpected null PathResolver");
            }
            return fileReader.PathResolver;
        }

        /// <summary>
        ///     Returns <see cref="IFileReaderWithResolver" /> from <see cref="Importer" /> instance on the
        ///     <paramref name="lessParser" />, if any. Otherwise raises exception.
        /// </summary>
        /// <param name="lessParser"></param>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="Parser.Importer" /> or <see cref="Importer.FileReader" /> are
        ///     not of expected types.
        /// </exception>
        /// <returns><see cref="IFileReaderWithResolver" /> instance.</returns>
        internal static IFileReaderWithResolver GetFileReaderWithResolver(this Parser lessParser)
        {
            var importer = lessParser.Importer as Importer;
            if (importer == null)
            {
                throw new InvalidOperationException(string.Format("{0} expected, encountered {1}", typeof (Importer), lessParser.Importer));
            }

            var fileReader = importer.FileReader as IFileReaderWithResolver;
            if (fileReader == null)
            {
                throw new InvalidOperationException(string.Format(
                    "{0} expected, encountered {1}. Please set the <dotless source /> attribute in web.config to a type implementing {0}",
                    typeof (IFileReaderWithResolver), importer.FileReader));
            }
            return fileReader;
        }

        /// <summary>
        ///     Un-decorates <see cref="LessEngine" /> from <paramref name="engine" />. Can process only
        ///     <see cref="CacheDecorator" /> and <see cref="ParameterDecorator" />, otherwise raises exception.
        /// </summary>
        /// <param name="engine">Possibly decorated <see cref="ILessEngine" /></param>
        /// <exception cref="ArgumentException">Unexpected type of <paramref name="engine" /></exception>
        /// <returns><see cref="LessEngine" /> instance.</returns>
        internal static LessEngine ResolveLessEngine(this ILessEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            while (true)
            {
                var lessEngine = engine as LessEngine;
                if (lessEngine != null)
                {
                    return lessEngine;
                }

                var cacheDecorator = engine as CacheDecorator;
                if (cacheDecorator != null)
                {
                    engine = cacheDecorator.Underlying;
                }
                else
                {
                    var parameterDecorator = engine as ParameterDecorator;
                    if (parameterDecorator == null)
                    {
                        throw new ArgumentException(string.Format("Cannot resolve {0} to LessEngine", engine.GetType()));
                    }
                    engine = parameterDecorator.Underlying;
                }
            }
        }
    }
}
