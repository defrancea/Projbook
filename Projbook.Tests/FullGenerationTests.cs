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
            new ProjbookEngine("../..", "Projbook/Example/Template/template.html", "projbook.json", ".").Generate();
        }
    }
}
