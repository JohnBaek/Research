namespace CreepTesterParser_V6.Services;

/// <summary>
/// 채널별 "마지막 데이터 시각"이 지금으로부터 신선도(freshness) 이내면 동작중, 아니면 정지중으로 판정.
/// CSV 의 PC_TIME 이 절대시각이라 이전 표본과 비교할 필요 없이 한 번 읽어 판정한다.
/// </summary>
public sealed class StatusMonitor
{
    public enum RunState
    {
        Running,  // 동작중 (최근 데이터 있음)
        Stopped,  // 정지중 (데이터가 오래됨)
        NoData,   // 파일 없음/데이터 없음
    }

    public sealed record Row(int Index, string? Ip, RunState State, DateTime? LastActivity, TimeSpan? Age);

    private readonly TimeSpan _freshness;

    /// <param name="freshness">마지막 데이터가 이 시간 이내면 동작중으로 본다(기본 30분).</param>
    public StatusMonitor(TimeSpan? freshness = null)
        => _freshness = freshness ?? TimeSpan.FromMinutes(30);

    public IReadOnlyList<Row> Evaluate(
        IReadOnlyList<Models.ChannelInfo> channels,
        IReadOnlyDictionary<int, DateTime?> lastActivity,
        DateTime now)
    {
        var rows = new List<Row>(channels.Count);
        foreach (var ch in channels)
        {
            lastActivity.TryGetValue(ch.Index, out var last);
            if (last is null)
            {
                rows.Add(new Row(ch.Index, ch.Ip, RunState.NoData, null, null));
                continue;
            }

            var age = now - last.Value;
            var state = age <= _freshness ? RunState.Running : RunState.Stopped;
            rows.Add(new Row(ch.Index, ch.Ip, state, last, age));
        }
        return rows;
    }
}
