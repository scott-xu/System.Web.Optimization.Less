using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace MvcApplication1.BootstrapTheme
{
    /// <summary>
    ///     Custom VirtualPathProvider for Bootstrap theme handling. It processes only variables.less file path
    /// </summary>
    public sealed class BootstrapThemeVirtualPathProvider : VirtualPathProvider
    {
        private const string BOOTSTRAP_LESS_ROOT = "~/Content/less/";
        private const string DEFAULT_THEME_NAME = "default";

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

        private bool FileExistsInternal(string virtualPath)
        {
            virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return virtualPath.Equals(_bootstrapVariablesLessPath, StringComparison.InvariantCultureIgnoreCase);
        }

        private VirtualFile GetFileInternal(string virtualPath)
        {
            // TODO: process other file paths when required. Now works only for variable.less file.
            return new VirtualPathFile(GetVariablesFileName());
        }

        private string GetCacheKeyInternal(string virtualPath)
        {
            return string.Format("{0}({1})", VirtualPathUtility.ToAppRelative(virtualPath).ToLowerInvariant(), CurrentThemeName);
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
        ///     Composes the physical file name, according to <see cref="CurrentThemeName" />.
        /// </summary>
        private static string GetVariablesFileName()
        {
            string themeFragment = (string.IsNullOrEmpty(_currentThemeName) || _currentThemeName == DEFAULT_THEME_NAME ? "" : "." + _currentThemeName);
            return string.Format("{0}variables{1}.less", BOOTSTRAP_LESS_ROOT, themeFragment);
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
    }
}
