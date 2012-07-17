using System;
using System.Collections.Generic;
using System.IO;
using NuGet;

namespace Deployd.Core
{
    public class LightPackageInformation : IPackage
    {
        public LightPackageInformation(string id, string version)
        {
            Id = id;
            Version = SemanticVersion.Parse(version);
        }

        public LightPackageInformation(string id, SemanticVersion version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; private set; }

        public SemanticVersion Version { get; private set; }

        public string Title
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> Authors
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> Owners
        {
            get { throw new NotImplementedException(); }
        }

        public Uri IconUrl
        {
            get { throw new NotImplementedException(); }
        }

        public Uri LicenseUrl
        {
            get { throw new NotImplementedException(); }
        }

        public Uri ProjectUrl
        {
            get { throw new NotImplementedException(); }
        }

        public bool RequireLicenseAcceptance
        {
            get { throw new NotImplementedException(); }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public string Summary
        {
            get { throw new NotImplementedException(); }
        }

        public string ReleaseNotes
        {
            get { throw new NotImplementedException(); }
        }

        public string Language
        {
            get { throw new NotImplementedException(); }
        }

        public string Tags
        {
            get { throw new NotImplementedException(); }
        }

        public string Copyright
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<PackageDependencySet> DependencySets
        {
            get { throw new NotImplementedException(); }
        }

        public Uri ReportAbuseUrl
        {
            get { throw new NotImplementedException(); }
        }

        public int DownloadCount
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IPackageFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public Stream GetStream()
        {
            throw new NotImplementedException();
        }

        public bool IsAbsoluteLatestVersion
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsLatestVersion
        {
            get { throw new NotImplementedException(); }
        }

        public bool Listed
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset? Published
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences
        {
            get { throw new NotImplementedException(); }
        }
    }
}