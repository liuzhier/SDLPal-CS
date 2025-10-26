namespace ModTools.Record;

public record class TsConfig(
    TsCompilerOptions compilerOptions,
    string[] files
);

public record class TsCompilerOptions
{
    public  readonly    string          target = "es2024";
    public  readonly    string          module = "NodeNext";
    public  readonly    string          moduleResolution = "NodeNext";
    public  readonly    bool            strict = true;
    public  readonly    bool            esModuleInterop = true;
    public  readonly    bool            skipLibCheck = true;
    public  readonly    string[]        lib = ["es2024"];
    public  readonly    string[]        types = [];
}
