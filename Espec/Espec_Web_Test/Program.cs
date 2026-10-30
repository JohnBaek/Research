using System.Diagnostics;
using System.Net;

namespace Espec_Web_Test;

class Program
{
    static async Task Main(string[] args)
    {
        // 사용법: dotnet run -- <장비IP> [반복횟수] [타임아웃초]
        string ipAddress = args.Length > 0 ? args[0] : "192.168.100.17";
        int repeat = args.Length > 1 ? int.Parse(args[1]) : 1;
        int timeoutSeconds = args.Length > 2 ? int.Parse(args[2]) : 30;

        for (int i = 1; i <= repeat; i++)
        {
            Console.WriteLine($"===== [{i}/{repeat}] {ipAddress} =====");

            // 매 회차마다 새 쿠키/커넥션으로 시작한다. (운영 코드와 같은 핸들러 설정, 리다이렉트만 수동)
            CookieContainer cookieContainer = new CookieContainer();
            using HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = false,
            };
            using HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            await LoginAsync(client, cookieContainer, ipAddress);

            if (i < repeat)
                await Task.Delay(1000);
        }
    }

    /// <summary>
    /// 로그인 POST → 302 Location 따라가기까지 단계별로 시간을 측정해 출력한다.
    /// </summary>
    private static async Task LoginAsync(HttpClient client, CookieContainer cookieContainer, string ipAddress)
    {
        Uri loginUri = new Uri($"http://{ipAddress}/login.php");

        FormUrlEncodedContent loginData = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", "Administrator"),
            new KeyValuePair<string, string>("password", "espec"),
            new KeyValuePair<string, string>("login_button", "내용 동의 후 로그인")
        ]);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, loginUri) { Content = loginData };
        request.Headers.Referrer = loginUri;

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            // 1. POST login.php — 헤더 수신까지
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Console.WriteLine($"[POST login.php] headers  {stopwatch.ElapsedMilliseconds,6} ms  {(int)response.StatusCode} {response.StatusCode}");

            // 1-1. 본문 수신까지
            string loginBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[POST login.php] body     {stopwatch.ElapsedMilliseconds,6} ms  {loginBody.Length} chars");

            Uri? location = response.Headers.Location;
            Console.WriteLine($"  Location : {location?.ToString() ?? "(없음)"}");
            Console.WriteLine($"  Cookies  : {string.Join("; ", cookieContainer.GetCookies(loginUri).Select(c => $"{c.Name}={c.Value}"))}");

            if (location == null)
            {
                Console.WriteLine("  → 302 가 아님. 로그인 실패 가능성 (본문 앞부분):");
                Console.WriteLine(loginBody[..Math.Min(500, loginBody.Length)]);
                return;
            }

            // 2. 리다이렉트 대상 GET (브라우저의 wm01.php)
            Uri nextUri = new Uri(loginUri, location);
            stopwatch.Restart();
            HttpResponseMessage nextResponse = await client.GetAsync(nextUri, HttpCompletionOption.ResponseHeadersRead);
            Console.WriteLine($"[GET {nextUri.AbsolutePath}] headers  {stopwatch.ElapsedMilliseconds,6} ms  {(int)nextResponse.StatusCode} {nextResponse.StatusCode}");

            string nextBody = await nextResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[GET {nextUri.AbsolutePath}] body     {stopwatch.ElapsedMilliseconds,6} ms  {nextBody.Length} chars");
            Console.WriteLine($"  로그인 페이지로 되돌아감? {nextUri.AbsolutePath.Contains("login", StringComparison.OrdinalIgnoreCase)}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[FAILED] {stopwatch.ElapsedMilliseconds} ms  {e.GetType().Name}: {e.Message}");
        }
    }
}
