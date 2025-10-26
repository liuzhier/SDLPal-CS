using SDLPal.Record.RGame;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SimpleUtility;

[JsonSerializable(typeof(short[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(List<string[]>))]
[JsonSerializable(typeof(HashSet<string[]>))]
[JsonSerializable(typeof(short[]))]
[JsonSerializable(typeof(BattleField))]
[JsonSerializable(typeof(HeroActionEffect))]
[JsonSerializable(typeof(Hero))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(Magic))]
[JsonSerializable(typeof(Enemy))]
[JsonSerializable(typeof(Poison))]
[JsonSerializable(typeof(Scene))]
[JsonSerializable(typeof(Event))]
public partial class JsonAuto : JsonSerializerContext;
public class JsonAutoEncoder : JavaScriptEncoder
{
    public override int MaxOutputCharactersPerInputCharacter => 1;

    public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
    {
        // 返回 -1 表示不编码任何字符
        return -1;
    }

    public override bool WillEncode(int unicodeScalar)
    {
        // 返回 false 表示不编码任何字符
        return false;
    }

    public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
    {
        numberOfCharactersWritten = 0;
        return false;
    }
}
