using System;
using System.Text;

namespace FluentParserNet {
public class FluentParser {
    private string _string {get; }
    private int _position {get; set; }
    public int Position => _position;
    public char NextChar => _string[_position];
    public bool HasNext => _position < _string.Length - 1;
    public char NextNextChar => CharactersLeft > 1 ? _string[_position + 1] : '\0';
    public int Length => _string.Length;
    public bool HasCurrent => _string.Length > _position;
    public int CharactersLeft => Length - _position;

    public FluentParser(string @string) => _string = @string;

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

    public bool
    Next(char @char) => NextChar == @char;

    public bool
    Next(string @string, int offset = 0) {

        if (@string.Length > CharactersLeft)
            return false;

        if (_string[_position + offset] != @string[0])
            return false;

        for (int i = offset + 1; i < @string.Length; i++)
            if (@string[i] != _string[_position + i])
                return false;
        return true;
    }

    public char
    NextCharAt(int offset) => _string[_position + offset];


    public bool
    NextIsSpace => NextChar == ' ';

    public char 
    ReadOne() {
        var result = NextChar;
        _position++;
        return result;
    }

    public string
    Read(int count) {
        count = Math.Min(count, CharactersLeft);
        var result = _string.Substring(_position, count);
        _position += count;
        return result;
    } 

    private string
    Read(FluentParser other) => this.Read(other.Position - _position);

    public string 
    ReadUntil(char @char, int maxLength) {
        if (NextChar == @char)
            return string.Empty;
        var end = Math.Min(_position + maxLength, Length);
        for (int i =_position; i < end; i++)
            if (_string[i] == @char){
                var result = _string.Substring(_position, i - _position);
                _position = i;
                return result;
            }
        return string.Empty;
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

        var index = _string.IndexOf(@char, _position, Math.Min(maxLength, CharactersLeft));
        if (index == -1) {
            result = null;
            return false;
        }
        
        result = _string.Substring(_position, index- _position);
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
        var index = _string.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1) {
            result = null;
            return false;
        }
        
        result = _string.Substring(_position, index- _position);
        _position = index;
        return true;
    }

    public bool
    TryLookWord(out string result, int offset = 0) => 
        new FluentParser(_string).Skip(_position).TryReadWord(out result, offset);

    public bool 
    TryReadWord(out string result, int offset = 0) {
        if (!NextCharAt(offset).IsWordCharacter()) {
            result = string.Empty;
            return false;
        }

        for (int i = _position + offset; i < Length; i++) {
            if (!_string[i].IsWordCharacter()) {
                result = _string.Substring(_position + offset, i - _position - offset);
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
                result = _string.Substring(_position + offset, i - _position - offset);
                return true;
            }
            if (!_string[i].IsWordCharacter())
                break;
        }

        return false;
    }
    
    public FluentParser 
    SkipUntil(char @char) {
        if (HasCurrent) {
            var index = _string.IndexOf(@char, _position);
            _position = index == -1 ? _string.Length : index;
        }
        return this;
    }

    public FluentParser
    SkipAfter(char @char) {
        if (HasCurrent) {
            var index = _string.IndexOf(@char, _position);
            _position = index == -1 ? _string.Length : index + 1;
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

    public FluentParser
    Clone() => new FluentParser(_string).Skip(_position);

    public override string 
    ToString() {
        int startIndex = Math.Max(_position - 20, 0);
        int endIndex = Math.Min(Length, startIndex + 40);
        var result = new StringBuilder();
        for (int i = startIndex; i < endIndex; i++) {
            if (i == _position)
                result.Append("*");
            result.Append(_string[i]);
        }
        return result.ToString();
    }
}

internal static class
FluentParserHelper {
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
}
}
