using System.Text.Json;
using Espec_EHS_222;

string ip = args.Length > 0 ? args[0] : "192.168.100.11";
int chamberAddr = 1;  // ← 챔버 주소 (기본 1, 여러 대면 1~8)

EspecRawHttpClient http = new(ip);

// ===== 1. 로그인 =====
Console.WriteLine("--- Login ---");
long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
EspecResponse loginResp = await http.GetAsync(
    $"/app/app.app?cmd=authority_login&username=Administrator&userpass=espec&chache={ts}");

using (JsonDocument loginDoc = JsonDocument.Parse(loginResp.Body))
{
    string result = loginDoc.RootElement.GetProperty("login_result").GetString() ?? "";
    Console.WriteLine(result == "success" ? "✅ 로그인 성공" : $"❌ {result}");
    if (result != "success") return;
}

// ===== 2. 챔버 주소를 세션에 저장 (핵심!) =====
Console.WriteLine($"\n--- Save addr={chamberAddr} to session ---");
long ts_save = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
EspecResponse saveResp = await http.GetAsync(
    $"/app/app.app?cmd=save_addr&addr={chamberAddr}&c={ts_save}");
Console.WriteLine($"Response: {saveResp.Body}");

// ===== 3. 모니터링 =====
Console.WriteLine("\n--- Monitoring ---");
long ts2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
EspecResponse monResp = await http.GetAsync(
    $"/app/hast.app?mon=%5B%5D&cset=%5B%5D&chache={ts2}");

Console.WriteLine($"Raw: {monResp.Body}\n");

using JsonDocument monDoc = JsonDocument.Parse(monResp.Body);
JsonElement root = monDoc.RootElement;

// error 체크
if (root.TryGetProperty("error", out JsonElement err))
{
    Console.WriteLine($"❌ 서버 에러: {err.GetProperty("exception").GetString()}");
    return;
}

JsonElement mon = root.GetProperty("mon");
JsonElement cset = root.GetProperty("cset");

decimal currentTemp = mon.GetProperty("temp").GetDecimal();
decimal setTemp = cset.GetProperty("temp").GetDecimal();
decimal currentHumi = mon.TryGetProperty("humi", out var mh) ? mh.GetDecimal() : 0m;
decimal setHumi = cset.TryGetProperty("humi", out var ch) ? ch.GetDecimal() : 0m;
string mode = mon.GetProperty("mode").GetString() ?? "UNKNOWN";

Console.WriteLine($"운전 모드   : {mode}");
Console.WriteLine($"현재 온도   : {currentTemp}°C  (설정: {setTemp}°C)");
Console.WriteLine($"현재 습도   : {currentHumi}%   (설정: {setHumi}%)");

Console.WriteLine("\nDone.");
Console.ReadKey();