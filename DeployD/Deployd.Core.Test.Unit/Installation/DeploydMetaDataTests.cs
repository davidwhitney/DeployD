using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deployd.Core.Installation;
using Moq;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.Installation
{
    [TestFixture]
    public class DeploydMetaDataTests
    {
        [Test]
        public void CanReadDeployDMetaDataFromNuspecFile()
        {
            var deploydMetadata = new DeployDMetaData("deployd.xml");
            Assert.That(deploydMetadata.ServiceName, Is.EqualTo("sample service"));
            Assert.That(deploydMetadata.IISPath, Is.EqualTo("somewebsite.com/virtual_directory"));
        }
    }
}
