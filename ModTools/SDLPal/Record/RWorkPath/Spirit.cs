namespace SDLPal.Record.RWorkPath;

public record class Spirit(
    string PathName,
    string Animation,
    string Avatar,
    string Character,
    string Item,
    FightSpirit Fight,
    UiSpirit Ui
);

public record class FightSpirit(
    string PathName,
    string HeroActionEffect,
    string Background,
    string Enemy,
    string Hero,
    string Magic
);

public record class UiSpirit(
    string PathName,
    string Menu,
    string DialogueCursor
);
