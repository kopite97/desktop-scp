using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SiteDesktop.App;

public sealed class ScpSettingsStore(string? path)
{
    private JsonObject _entries=new();
    private bool _protected;
    public string Status { get; private set; }="";
    public IReadOnlyDictionary<string,bool> Load()
    {
        var result=new Dictionary<string,bool>();
        if(path==null||!File.Exists(path))return result;
        try
        {
            var root=JsonNode.Parse(File.ReadAllText(path))?.AsObject()??throw new JsonException();
            if(root["version"]?.GetValue<int>()!=1)throw new JsonException("Unsupported settings version");
            _entries=root["enabled"]?.AsObject()??throw new JsonException();
            foreach(var pair in _entries)if(pair.Value is JsonValue value&&value.TryGetValue<bool>(out var enabled))result[pair.Key]=enabled;
            if(result.Count!=_entries.Count)Status="일부 설정값을 읽을 수 없어 기본값을 사용합니다.";
        }
        catch(Exception ex)when(ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or FormatException)
        {
            _protected=true;Status="설정을 읽지 못해 기본값을 사용합니다. 원본을 보존하고 복구 파일에 저장합니다.";
            string recovery=path+".recovered.json";
            if(File.Exists(recovery))
            {
                try
                {
                    var recovered=JsonNode.Parse(File.ReadAllText(recovery))?.AsObject();
                    if(recovered?["version"]?.GetValue<int>()==1&&recovered["enabled"] is JsonObject entries)
                    {
                        _entries=entries;result.Clear();
                        foreach(var pair in entries)if(pair.Value is JsonValue value&&value.TryGetValue<bool>(out var enabled))result[pair.Key]=enabled;
                        Status="원본 설정은 보존하고 이전 복구 설정을 사용합니다.";
                    }
                }
                catch(Exception recoveryError)when(recoveryError is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or FormatException){}
            }
        }
        return result;
    }
    public bool Save(IReadOnlyDictionary<string,bool> settings)
    {
        if(path==null)return true;
        string target=_protected?path+".recovered.json":path;
        string temporary=target+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            var entries=(JsonObject)_entries.DeepClone();foreach(var pair in settings)entries[pair.Key]=pair.Value;
            var root=new JsonObject{["version"]=1,["enabled"]=entries};
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(target))!);
            File.WriteAllText(temporary,root.ToJsonString(new JsonSerializerOptions{WriteIndented=true}));
            File.Move(temporary,target,true);_entries=(JsonObject)entries.DeepClone();
            Status=_protected?"원본 설정은 보존했습니다. 복구 설정은 scp-settings.json.recovered.json에 저장됩니다.":"설정이 저장되었습니다.";return true;
        }
        catch(Exception ex)when(ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {Status="설정 저장 실패 · 현재 선택은 이번 실행 동안 유지됩니다.";return false;}
        finally{try{if(File.Exists(temporary))File.Delete(temporary);}catch(IOException){}catch(UnauthorizedAccessException){}}
    }
}
