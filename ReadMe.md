# FluentParser .NET

FluentParserNet is a convenient and efficient string parser on .NET written in a fluent style.

## Usage 

Basic read
```c#
var reader = new FluentReader("Fight of the Year #1: Jeff Dean vs. Chunk Norris");
var fightNumber = reader.SkipAfter('#').ReadInt();
var fighter1 = reader.SkipAfter(':').SkipSpaces().ReadUntil(" vs.");
var fighter2 = reader.Skip(" vs. ".Length).ReadTillEnd();
```

Try read an XML node from the current position. Position remains the same if read failed.
```c#
var reader = new FluentReader("Some useless text <Root> <Child id="1" /> <Root/> some other useless text")
if (reader.SkipUntil('<').TryReadXml(out var xml)) {
    Console.WriteLine(xml); //<Root> <Child id="1" /> <Root/>
    Console.WriteLine(reader); //...useless text <Root> <Child id="1" /> <Root/>* some other us...
}
else 
    Console.WriteLine(reader); //...useless text *<Root> <Child id="1" /> <Root/> some other us...
```


