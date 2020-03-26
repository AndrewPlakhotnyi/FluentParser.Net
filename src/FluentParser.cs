using System;

namespace FluentParser.Net {
public class FluentParser {
    private string _string {get; }
    private int _position {get; set; }
    public char NextChar => _string[_position];
    public bool HasNext => _position < _string.Length - 1;
    public int Length => _string.Length;
    public bool HasCurrent => _string.Length > _position;

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
    TryReadUntil(char @char, int maxLength, out string result) {
        var index = _string.IndexOf(@char, _position, maxLength);
        if (index == -1) {
            result = null;
            return false;
        }
        
        result = _string.Substring(_position, index- _position);
        _position = index;
        return true;
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
    
    public FluentParser SkipUntil(char @char) {
        if (HasCurrent) {
            var index = _string.IndexOf(@char, _position);
            _position = index == -1 ? _string.Length : index
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
}
}
