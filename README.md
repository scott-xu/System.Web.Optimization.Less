System.Web.Optimization.Less
============================
PM> Install-Package System.Web.Optimization.Less

Sample:

```
BundleTable.Bundles.Add(new LessBundle("~/Content/bootstrap").Include(
  "~/Content/less/bootstrap.less",
  "~/Content/less/responsive.less"));
```

## VirtualPathProvider support

Dotless relies on `VirtualPathProvider` for getting less file sources. `System.Web.Optimization` itself uses this approach heavily. In `LessTransform` it is used also for imported files (`@import "virtual path"` less directive). 

### Transient virtual files

By creating a custom `VirtualPathProvider` it is very easy to implement transient or context dependent *.less* files. For example, it is possible to compile different Bootstrap themes for different clients using the same *less* bundle. See the demo web application where the custom `BootstrapThemeVirtualPathProvider` handles `variables.less` which is imported in the bundled files.


License: <a href="http://opensource.org/licenses/MIT">MIT</a>
