System.Web.Optimization.Less
============================
PM> Install-Package System.Web.Optimization.Less

Sample:

BundleTable.Bundles.Add(new LessBundle("~/Content/bootstrap").Include("~/Content/less/bootstrap.less", "~/Content/less/responsive.less"));
