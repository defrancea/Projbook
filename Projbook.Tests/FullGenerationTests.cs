using NUnit.Framework;
using Projbook.Core;

namespace Projbook.Tests
{
    [TestFixture]
    public class FullGenerationTests
    {
        [Test]
        [TestCase]
        public void FullGeneration()
        {
            new ProjbookEngine("../..", "Resources/Example/template.html", "Resources/Example/projbook.json", ".").Generate();
        }
    }
}
