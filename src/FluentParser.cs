using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FluentParserNet {
public class FluentParser {
    public string String {get; }
    private int _position {get; set; }
    public int Position => _position;
    public char NextChar => String[_position];
    public bool HasNext => _position < String.Length - 1;
    public char NextNextChar => CharactersLeft > 1 ? String[_position + 1] : '\0';
    public char PreviousChar => String[_position -1];
    public int Length => String.Length;
    public bool HasCurrent => String.Length > _position;
    public int CharactersLeft => Length - _position;

    public FluentParser(string @string) => String = @string;

    public FluentParser
    SkipOne() {
        _position++;
        return this;
    }

    public FluentParser
    Skip(int count) {
        _position+= count;
        return this;
    }

    public FluentParser
    SkipSpaces() {
        while(HasNext && NextChar == ' ')
            SkipOne();
        return this;
    }

    public FluentParser
    SkipToDigit() {
        while(HasCurrent && !NextChar.IsDigit())
            SkipOne();
        return this;
    }

    public FluentParser 
    SkipToEnd() {
        _position = String.Length;
        return this;
    }

    public FluentParser
    RollbackUntilSpace() => RollbackUntil(" ");

    public FluentParser
    RollbackOne() => Skip(-1);

    public FluentParser
    Rollback(int count) => Skip(-count);

    public FluentParser
    RollbackUntil(string @string) {
        var index = String.LastIndexOf(@string, Position, StringComparison.Ordinal);
        if (index == -1) 
            _position = 0;
        _position = index + @string.Length;
        return this;
    }

    public bool
    Next(char @char) => NextChar == @char;

    public bool
    Next(string @string, int offset = 0) {

        if (@string.Length > CharactersLeft)
            return false;

        if (String[_position + offset] != @string[0])
            return false;

        for (int i = offset + 1; i < @string.Length; i++)
            if (@string[i] != String[_position + i])
                return false;
        return true;
    }

    public bool
    NextCaseInSensitive(string @string, int offset = 0) {
        if (@string.Length > CharactersLeft)
            return false;

        var result = this.Clone().Read(@string.Length);
        return result.Equals(@string,comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    public char
    NextCharAt(int offset) => String[_position + offset];

    public bool
    NextIsSpace => NextChar == ' ';

    public bool 
    NextIsDigit => NextChar.IsDigit();

    public char 
    ReadOne() {
        var result = NextChar;
        _position++;
        return result;
    }

    public string
    Read(int count) {
        count = Math.Min(count, CharactersLeft);
        var result = String.Substring(_position, count);
        _position += count;
        return result;
    } 

    public string 
    ReadUntilPosition(int position) {
        if (position < Position)
            throw new ArgumentException($"Can't read until position {position} because now the reader is at the position {Position}");
        return Read(count: position - Position);
    }

    private string
    Read(FluentParser other) => this.Read(other.Position - _position);

    public string 
    ReadUntil(char @char, int maxLength) {
        if (NextChar == @char)
            return string.Empty;
        var end = Math.Min(_position + maxLength, Length);
        for (int i =_position; i < end; i++)
            if (String[i] == @char){
                var result = String.Substring(_position, i - _position);
                _position = i;
                return result;
            }
        return string.Empty;
    }

    public string 
    ReadUntil(char @char) {
        if (!HasNext || NextChar == @char)
            return string.Empty;
        for (int i =_position; i < Length; i++)
            if (String[i] == @char){
                var result = String.Substring(_position, i - _position);
                _position = i;
                return result;
            }
        return string.Empty;
    }

    public string 
    ReadBackUntil(char @char) {
        if (Position == 0 || PreviousChar == @char)
            return string.Empty;
        for(int i = _position - 1; i >=0; i--)
            if (String[i] == @char) {
                var result = String.Substring(i + 1, _position - i - 1);
                _position = i + 1;
                return result;
            }

        return string.Empty;
    }

    public string 
    ReadBackAll() => String.Substring(0, _position);

    public string 
    ReadUntil(string @string) {
        var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1 || index == _position)
            return String.Empty;
        var result = String.Substring(_position, index - _position);
        _position = index;
        return result;
    }

    public string
    ReadUntilLast(char @char) {
        var index = String.LastIndexOf(@char);
        if (index == -1 || index == _position)
            return string.Empty;
        var result =  String.Substring(Position, index - Position);
        _position = index;
        return result;
    }

    public bool
    TryReadUntilSpace(out string result) => TryReadUntil(' ', int.MaxValue, out result);

    public bool 
    TryReadUntil(char @char, int maxLength, out string result) {
        if (NextChar == @char) {
            result = string.Empty;
            return true;
        }

        if (!HasNext) {
            result = null;
            return false;
        }

        var index = String.IndexOf(@char, _position, Math.Min(maxLength, CharactersLeft));
        if (index == -1) {
            result = null;
            return false;
        }
        
        result = String.Substring(_position, index- _position);
        _position = index;
        return true;
    }
    

    public bool
    TryReadAfter(string @string, out string result) {
        if (TryReadUntil(@string, out result)) {
            _position += @string.Length;
            result += @string;
            return true;
        }
        result = null;
        return false;
    }

    public bool 
    TryReadUntil(string @string,  out string result) {
        var initialPosition = _position;
        var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1) {
            result = null;
            return false;
        }
        
        result = String.Substring(_position, index- _position);
        _position = index;
        return true;
    }

    public bool 
    TrySkipAfter(string @string, StringComparison comparisonType = StringComparison.Ordinal) {
        var index = String.IndexOf(@string, _position, comparisonType);
        if (index == -1)
            return false;
        _position = index + @string.Length;
        return true;
    }

    public bool 
    TrySkipUntil(string @string) {
        var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1)
            return false;
        _position = index;
        return true;
    }

    public bool
    TryLookWord(out string result, int offset = 0) => 
        new FluentParser(String).Skip(_position).TryReadWord(out result, offset);

    public bool 
    TryReadWord(out string result, int offset = 0) {
        if (!NextCharAt(offset).IsWordCharacter()) {
            result = string.Empty;
            return false;
        }

        for (int i = _position + offset; i < Length; i++) {
            if (!String[i].IsWordCharacter()) {
                result = String.Substring(_position + offset, i - _position - offset);
                _position = i;
                return true;
            }
                
        }

        result = string.Empty;
        return false;
    }


    public bool
    TryLookWordUntil(string @string, out string result, int offset = 0) {
        result = string.Empty;
        if (Next(@string, offset))
            return true;

        if (!HasNext)
            return false;

        for (int i = _position + offset; i < Length; i++) {
            if (Next(@string, offset: i)) {
                result = String.Substring(_position + offset, i - _position - offset);
                return true;
            }
            if (!String[i].IsWordCharacter())
                break;
        }

        return false;
    }
    
    public FluentParser 
    SkipUntil(char @char) {
        if (HasCurrent) {
            var index = String.IndexOf(@char, _position);
            _position = index == -1 ? String.Length : index;
        }
        return this;
    }

    public FluentParser
    SkipUntilNextLine() {
        SkipAfter('\n');
        if (Next('\r'))
            SkipOne();
        return this;
    }

    public FluentParser
    SkipAfter(char @char) {
        if (HasCurrent) {
            var index = String.IndexOf(@char, _position);
            _position = index == -1 ? String.Length : index + 1;
        }
        return this;
    }

    public FluentParser
    SkipAfter(string @string) {
        if (HasCurrent) {
            var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
            _position = index == -1 ? String.Length : index + @string.Length ;
        }
        return this;
    }


    public bool
    TryReadXmlNode(out string result) {
        result = string.Empty;
        if (!Next('<') || CharactersLeft < 4)
            return false;

        var reader = this.Clone();
        if (!reader.SkipOne().TryReadWord(out var nodeName))
            return false;

        int openedNodes = 1;
        while(reader.HasNext) {
            var @char = reader.ReadOne();
            if (@char == '<') {
                if (reader.NextChar == '!') {
                    //this probably some kind of internal data  like <!CDATA[]>
                    reader.SkipAfter('>');
                    continue;
                }
                    
                if (reader.NextChar == '/') {
                    openedNodes--;
                    if (openedNodes == 0) {
                        if (reader.Next($"/{nodeName}>")) {
                            result = Read(reader.Skip(nodeName.Length + 2));
                            return true;
                        }
                        return false;
                    }
                }
                else 
                    openedNodes++;
            }
            else if (@char == '/' && reader.NextChar == '>') {
                openedNodes--;
                if (openedNodes == 0) {
                    result = Read(reader.SkipOne());
                    return true;
                }

            }
        }
        return false;
    }

    public bool 
    TryReadJson<T>(Func<string, T> parser, out T result) {
        result = default;
        if (!HasNext || NextChar != '{')
            return false;

        var initialPosition = _position;
        int braces = 0;
        while(HasNext) {
            var next = NextChar;
            SkipOne();
            if (next == '{')
                braces++;
            else if (next == '}') {
                braces--;
                if (braces == 0) {
                    try {
                        result = parser(String.Substring(initialPosition, Position - initialPosition));
                        return true;
                    }
                    catch(Exception) {
                        _position = initialPosition;
                        return false;
                    }
                }
            }
        }
        _position = initialPosition;
        return false;
    }

    public string
    ReadToEnd() => String.Substring(Position);

    public int
    ReadIntUntil(char @char) {
        if (!NextChar.IsDigit())
            throw new InvalidOperationException($"Read must be positioned at a digit but was {this}");
        var result = ReadDigit();
        while(NextChar != @char) result = result * 10 + ReadDigit();
        return result;
    }

    public int 
    ReadNextInt() => SkipToDigit().ReadInt();

    public int 
    ReadInt() {
        int result = 0;
        while(HasCurrent && NextChar.IsDigit()) {
            result = result * 10 + NextChar.ToDigit();
            SkipOne();
        }
        return result;
    }

    public long 
    ReadLong() {
        long result = 0;
        while(HasCurrent && NextChar.IsDigit()) {
            result = result * 10 + NextChar.ToDigit();
            SkipOne();
        }
        return result;
    }

    public double
    ReadNextDouble(CultureInfo cultureInfo = null) => SkipToDigit().ReadDouble(cultureInfo);

    public double 
    ReadDouble(CultureInfo cultureInfo = null) {
        if (!NextChar.IsDigit())
           throw new InvalidOperationException($"Reader position must be placed on a digit: {this}");
        double result = ReadInt();

        void 
        AddIntPart(int number, int digits) => result = result * Math.Pow(10,  digits) + number;
       
       (int number, int digits) 
        ReadIntLocal() {
           var initialPosition = Position;
           return (ReadInt(), Position - initialPosition);
       }

       while(HasCurrent) {
           if (Next('.') || Next(',')){
               if (!String[Position + 1].IsDigit())
                   return result;
               SkipOne();
               var (number, digits) = ReadIntLocal();
               if (digits == 3 && (cultureInfo == null || !Equals(cultureInfo, CultureInfo.InvariantCulture)))
                   AddIntPart(number, 3);
               else 
                   return result + number / Math.Pow(10, digits);
           }
           else 
               return result;
       }

       return result;
    }

    public int 
    ReadDigit() {
        var result = NextChar.ToDigit();
        SkipOne();
        return result;
    }

    public string 
    ReadHexString() {
        string result = string.Empty;
        while(HasNext) {
            var next = ReadOne();
            if (next.IsHexDigit())
                result += next.ToString();
            else 
                break;
        }
        return result;
    }

    public FluentParser
    Clone() => new FluentParser(String).Skip(_position);

    public override string 
    ToString() {
        int startIndex = Math.Max(_position - 20, 0);
        int endIndex = Math.Min(Length, startIndex + 40);
        var result = new StringBuilder();
        for (int i = startIndex; i < endIndex; i++) {
            if (i == _position)
                result.Append("*");
            result.Append(String[i]);
        }
        return result.ToString();
    }

    public FluentParser
    SkipFromEndUntil(char @char) {
        SkipToEnd().RollbackOne();
        while (NextChar != @char) {
            RollbackOne();
        }

        return this;
    }

    public FluentParser
    VerifyNext(string @string)  {
        if (!Next(@string))
            throw new InvalidOperationException($"Expecting next=\"{@string.ToString()}\" but was \"{this}\"");
        return this;
    }
}

internal static class
FluentParserHelperInternal {
    public static bool
    IsWordCharacter(this char @char) => @char.IsDigit() || @char.IsLetter();

    public static bool
    IsDigit(this char @char) => '0' <= @char && @char <= '9';

    public static bool
    IsCapitalLetter(this char @char) => 'A' <= @char && @char <= 'Z';

    public static bool
    IsSmallLetter(this char @char) => 'a' <= @char && @char <= 'z';

    public static bool
    IsLetter(this char @char) => @char.IsSmallLetter() || @char.IsCapitalLetter();

    public static int
    ToDigit(this char @char) {
        #if DEBUG
        if (@char - '0' > 9)
            throw new ArgumentOutOfRangeException(nameof(@char));
        #endif
        return  @char - '0';
    }

    public static bool 
    IsHexDigit(this char @char) => @char.IsDigit() || 'A' <= @char && @char <= 'F' || 'a' <= @char && @char <= 'f';
}

public static class 
FluentParserHelper {
    public static FluentParser
    ToFluentParser(this string @string) => new FluentParser(@string);

    public static long
    ParseLongFromHexString(this string @string)  => Convert.ToInt64(@string, 16);

    public static int
    ParseIntFromHexString(this string @string)  => Convert.ToInt32(@string, 16);
}

}
