namespace SDLPal.Record.RWorkPath;

public record class Game(
    string PathName,
    string Voice,
    string Musics,
    MapData MapData,
    Spirit Spirit,
    Data Data
);
