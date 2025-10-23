using SDLPal.Record.RGame;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SimpleUtility;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = false)]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(List<string[]>))]
[JsonSerializable(typeof(short[]))]
//[JsonSerializable(typeof(Shop))]
//[JsonSerializable(typeof(EnemyTeam))]
[JsonSerializable(typeof(BattleField))]
[JsonSerializable(typeof(HeroActionEffect))]
[JsonSerializable(typeof(Hero))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(Magic))]
[JsonSerializable(typeof(Enemy))]
[JsonSerializable(typeof(Poison))]
[JsonSerializable(typeof(Scene))]
[JsonSerializable(typeof(Event))]
public partial class JsonAuto : JsonSerializerContext
{
   public static JsonSerializerOptions? AutoOption { get; } = CreateCustomOptions();

   static JsonSerializerOptions CreateCustomOptions() => new JsonSerializerOptions
   {
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
      WriteIndented = true,
   };
}
