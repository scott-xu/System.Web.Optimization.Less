using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace MvcApplication1.DynamicFiles
{
    /// <summary>
    ///     Custom VirtualPathProvider for Bootstrap theme handling. It processes only variables.less file path
    /// </summary>
    public sealed class DynamicFileVirtualPathProvider : VirtualPathProvider
    {
        /// <summary>
        ///     Dynamic file type resolved from virtual path.
        /// </summary>
        private enum VirtualPathType
        {
            None,
            BootstrapVariables,
            JqueryGlobalizeCulture,
            JqueryGlobalizeReset
        }

        // jquery globalize
        private const string GLOBALIZE_ROOT = "~/Scripts/jquery.globalize/";

        /// <summary> App path of the dynamic file corresponding to the current culture jquery.globalize definition. </summary>
        public const string GLOBALIZE_CULTURE_PATH = GLOBALIZE_ROOT + "globalize.culture.js";

        /// <summary> App path of the dynamic file corresponding to the culture reset script. </summary>
        public const string GLOBALIZE_RESET_PATH = GLOBALIZE_ROOT + "globalize-reset";

        private const string GLOBALIZE_FILE_MASK = "~/Scripts/jquery.globalize/cultures/globalize.culture.{0}.js";
        private const string GLOBALIZE_RESET_CULTURE_SRC = @"
(function(g){{
    g.cultureSelector = '{0}';
    g.cultures.default = g.cultures['{0}'];
}})(Globalize);";

        private static CultureInfo _currentCulture = CultureInfo.GetCultureInfo("en-US");

        // bootstrap theming 
        private const string BOOTSTRAP_LESS_ROOT = "~/Content/less/";
        private const string DEFAULT_THEME_NAME = "default";
        private const string CONTENT_LESS_VARIABLES_LESS = BOOTSTRAP_LESS_ROOT + "variables{0}.less";

        private static IList<string> _themeNames = new string[0];
        private static string _currentThemeName = DEFAULT_THEME_NAME;
        private static readonly string _bootstrapVariablesLessPath = GetVariablesFileName();

        protected override void Initialize()
        {
            base.Initialize();
            PopulateThemeNames();
        }

        /// <summary>
        ///     Intercepts file existence check for variables.less path.
        /// </summary>
        public override bool FileExists(string virtualPath)
        {
            return FileExistsInternal(virtualPath) || base.FileExists(virtualPath);
        }

        /// <summary>
        ///     Intercepts getting file for variables.less path.
        /// </summary>
        public override VirtualFile GetFile(string virtualPath)
        {
            return FileExistsInternal(virtualPath) ? GetFileInternal(virtualPath) : base.GetFile(virtualPath);
        }

        /// <summary>
        ///     Intercepts getting cache key for variables.less path.
        /// </summary>
        public override string GetCacheKey(string virtualPath)
        {
            return FileExistsInternal(virtualPath) ? GetCacheKeyInternal(virtualPath) : base.GetCacheKey(virtualPath);
        }

        private static bool FileExistsInternal(string virtualPath)
        {
            return GetFileType(virtualPath) != VirtualPathType.None;
        }

        private static VirtualPathType GetFileType(string virtualPath)
        {
            virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            if (virtualPath.Equals(_bootstrapVariablesLessPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return VirtualPathType.BootstrapVariables;
            }
            if (virtualPath.Equals(GLOBALIZE_CULTURE_PATH, StringComparison.InvariantCultureIgnoreCase))
            {
                return VirtualPathType.JqueryGlobalizeCulture;
            }
            if (virtualPath.Equals(GLOBALIZE_RESET_PATH, StringComparison.InvariantCultureIgnoreCase))
            {
                return VirtualPathType.JqueryGlobalizeReset;
            }
            return VirtualPathType.None;
        }

        private static VirtualFile GetFileInternal(string virtualPath)
        {
            string physicalFileName = null;
            string content = null;
            switch (GetFileType(virtualPath))
            {
                case VirtualPathType.JqueryGlobalizeCulture:
                    physicalFileName = string.Format(GLOBALIZE_FILE_MASK, Thread.CurrentThread.CurrentCulture.Name);
                    break;
                case VirtualPathType.JqueryGlobalizeReset:
                    content = string.Format(GLOBALIZE_RESET_CULTURE_SRC, Thread.CurrentThread.CurrentCulture.Name);
                    break;
                case VirtualPathType.BootstrapVariables:
                    physicalFileName = GetVariablesFileName();
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unsupported file path: {0}", virtualPath));
            }
            return content == null ? (VirtualFile) new VirtualPathFile(physicalFileName) : new StringVirtualFile(virtualPath, content);
        }

        /// <summary>
        ///     Composes the physical file name, according to <see cref="CurrentThemeName" />.
        /// </summary>
        private static string GetVariablesFileName()
        {
            string themeFragment = (string.IsNullOrEmpty(_currentThemeName) || _currentThemeName == DEFAULT_THEME_NAME ? "" : "." + _currentThemeName);
            return string.Format(CONTENT_LESS_VARIABLES_LESS, themeFragment);
        }

        private static string GetCacheKeyInternal(string virtualPath)
        {
            string salt;
            switch (GetFileType(virtualPath))
            {
                case VirtualPathType.JqueryGlobalizeCulture:
                case VirtualPathType.JqueryGlobalizeReset:
                    salt = Thread.CurrentThread.CurrentCulture.Name;
                    break;
                case VirtualPathType.BootstrapVariables:
                    salt = CurrentThemeName;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unsupported file path: {0}", virtualPath));
            }
            return string.Format("{0}({1})", VirtualPathUtility.ToAppRelative(virtualPath).ToLowerInvariant(), salt);
        }

        /// <summary>
        ///     Finds all variables*.less files in bootstrap directory, catches the theme name from the file name and populates the
        ///     static list.
        /// </summary>
        private static void PopulateThemeNames()
        {
            var names = new List<string>();
            var themeMatcher = new Regex(@"\\variables(?:\.(\w+))?\.less$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            names.AddRange(
                Directory
                    .EnumerateFiles(HostingEnvironment.MapPath(BOOTSTRAP_LESS_ROOT), "variables*.less", SearchOption.TopDirectoryOnly)
                    .Select(path =>
                    {
                        Match match = themeMatcher.Match(path);
                        string name = match.Groups[1].Value;
                        return string.IsNullOrEmpty(name) ? DEFAULT_THEME_NAME : name;
                    })
                    .OrderBy(s => s));
            if (!names.Contains(DEFAULT_THEME_NAME))
            {
                throw new ApplicationException(string.Format("Required file {0} does not exist", _bootstrapVariablesLessPath));
            }
            ThemeNames = names;
            CurrentThemeName = DEFAULT_THEME_NAME;
        }

        /// <summary>
        ///     Gets or sets application-wide Bootstrap theme name.
        /// </summary>
        public static string CurrentThemeName
        {
            get { return _currentThemeName; }
            set
            {
                if (!_themeNames.Contains(value, StringComparer.InvariantCultureIgnoreCase))
                {
                    throw new ArgumentOutOfRangeException(string.Format("Bootstrap theme '{0}' not exists", value));
                }
                _currentThemeName = value;
            }
        }

        /// <summary>
        ///     Gets names of all available Bootstrap themes.
        /// </summary>
        public static IList<string> ThemeNames
        {
            get { return new ReadOnlyCollection<string>(_themeNames); }
            private set { _themeNames = new ReadOnlyCollection<string>(value); }
        }

        public static CultureInfo CurrentCulture
        {
            get { return _currentCulture; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _currentCulture = value;
            }
        }
    }
}
