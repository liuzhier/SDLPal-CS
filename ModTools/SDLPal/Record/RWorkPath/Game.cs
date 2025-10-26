namespace SDLPal.Record.RWorkPath;

public record class Game(
    string PathName,
    string Musics,
    string Palette,
    string Voice,
    MapData MapData,
    Spirit Spirit,
    Data Data
);
