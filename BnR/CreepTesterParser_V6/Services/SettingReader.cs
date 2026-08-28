using System.Text;
using CreepTesterParser_V6.Ini;
using CreepTesterParser_V6.Models;

namespace CreepTesterParser_V6.Services;

/// <summary>
/// Setting.ini 를 읽어 <see cref="CreepSetting"/> 으로 변환한다.
/// </summary>
public sealed class SettingReader
{
    // Setting.ini 는 CP949(EUC-KR)로 저장됨.
    private static readonly Encoding Cp949 = Encoding.GetEncoding(949);

    private const string SecInput = "InputMode_Set";
    private const string SecSave = "SAVE_MODE";
    private const string SecSetup = "SETUP";
    private const string SecSaveTime = "SAVE_TIME";

    /// <summary>파일이 없으면 <see cref="FileNotFoundException"/>.</summary>
    public CreepSetting Read(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Setting.ini 파일을 찾을 수 없습니다: {path}", path);

        var ini = IniDocument.Load(path, Cp949);

        var useCh = ParseInt(ini.Get(SecInput, "USE_CH")) ?? 0;

        var channels = new List<ChannelInfo>(useCh);
        for (var n = 1; n <= useCh; n++)
        {
            channels.Add(new ChannelInfo
            {
                Index = n,
                Ip = ini.Get(SecInput, $"IP{n}"),
                UserName = ini.Get(SecSave, $"USERNAME{n}"),
                ExpDate = ini.Get(SecSave, $"EXPDATE{n}"),
                ExpLoad = ini.Get(SecSave, $"EXPLOAD{n}"),
                ExpTemp = ini.Get(SecSave, $"EXPTEMP{n}"),
                GageLength = ini.Get(SecSave, $"GAGELENGTH{n}"),
                Stress = ini.Get(SecSave, $"STRESS{n}"),
                FilePath = ini.Get(SecSave, $"FILEPATH{n}"),
                FileName = ini.Get(SecSave, $"FILENAME{n}"),
                SaveFilePath = ini.Get(SecSave, $"SAVEFILEPATH{n}"),
                SaveContents = ini.Get(SecSave, $"SAVE_CONTENTS{n}"),
                TestTimerSec = ParseDouble(ini.Get(SecSaveTime, $"TIMER{n}")),
                DataCount = ParseLong(ini.Get(SecSaveTime, $"DATA{n}")),
            });
        }

        return new CreepSetting
        {
            SourcePath = path,
            UseChannelCount = useCh,
            LoadUnit = ini.Get(SecSetup, "UNIT1"),
            LeverRatio = ini.Get(SecInput, "RATIO"),
            ComMode = ini.Get(SecInput, "COM_MODE"),
            ComPort = ini.Get(SecInput, "COMPORT"),
            StartNumber = ini.Get(SecInput, "START_NUMBER"),
            ActiveChannels = channels,
        };
    }

    private static int? ParseInt(string? s)
        => int.TryParse(s, out var v) ? v : null;

    private static double? ParseDouble(string? s)
        => double.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;

    private static long? ParseLong(string? s)
        => long.TryParse(s, out var v) ? v : null;
}
