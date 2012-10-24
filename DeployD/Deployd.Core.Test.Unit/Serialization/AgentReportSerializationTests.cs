using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using Deployd.Core.Installation;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.Serialization
{
    [TestFixture]
    public class AgentReportSerializationTests
    {
        [Test]
        public void Serialized_IncludesInstallationResult()
        {
            var report = new AgentStatusReport()
                             {
                                 packages=new List<LocalPackageInformation>()
                                              {
                                                  new LocalPackageInformation(){InstallationResult = new InstallationResult(){Failed=true}}
                                              }
                             };

            string output = "";
            var serializer = new DataContractJsonSerializer(typeof(AgentStatusReport));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, report);
                ms.Position = 0;
                using (var streamREader = new StreamReader(ms))
                {
                    output = streamREader.ReadToEnd();
                }
            }
            System.Diagnostics.Debug.WriteLine(output);
            Assert.That(output, Is.StringContaining(@"""installationResult"":{""failed"":true}"));
            
        }

        [Test]
        public void DeSerialized_IncludesInstallationResult()
        {
            var serializer = new DataContractJsonSerializer(typeof (AgentStatusReport));
            
            AgentStatusReport report = null;

            using (var file = new FileStream("statusReport.txt", FileMode.Open, FileAccess.Read))
            {
                report = serializer.ReadObject(file) as AgentStatusReport;
            }
            var nmpClient = report.packages.SingleOrDefault(p => p.PackageId == "GG.Integration.EmailVision.NmpClient");
            Assert.That(nmpClient, Is.Not.Null);

            var installationResult = nmpClient.InstallationResult;

            Assert.That(installationResult, Is.Not.Null);
            Assert.That(installationResult.Failed, Is.True);
        }
    }
}