using NUnit.Framework;

namespace FluentParserNet.Tests {
public static class FluentParserTests {
 

    [TestCase("<Test />", "<Test />")]
    [TestCase("<Test />aaaa", "<Test />")]
    [TestCase("<<Test />", "")]
    [TestCase("<Root> <Inner></Inner> </Root></Root>", "<Root> <Inner></Inner> </Root>")]
    [TestCase("<Root/>", "<Root/>")]
    [TestCase("<Root><!CDATA></Root>", "<Root><!CDATA></Root>")]
    public static void
    TryParseXmlNodeTest(string text, string expectedResult) {
        new FluentParser(text).TryReadXmlNode(out var actual);
        Assert.AreEqual(expectedResult, actual);
    }
    
}
}