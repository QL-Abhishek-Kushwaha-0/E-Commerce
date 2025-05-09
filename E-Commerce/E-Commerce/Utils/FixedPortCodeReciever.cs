using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Requests;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

public class FixedPortCodeReceiver : ICodeReceiver
{
    public string RedirectUri => "http://127.0.0.1:5000/authorize/"; // Set fixed port 5000

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken cancellationToken)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri);
        listener.Start();

        Process.Start(new ProcessStartInfo
        {
            FileName = url.Build().ToString(),
            UseShellExecute = true
        });

        var context = await listener.GetContextAsync();
        var response = context.Response;
        var code = context.Request.QueryString["code"];

        var responseString = "Authentication successful. You may close this window.";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        listener.Stop();

        return new AuthorizationCodeResponseUrl { Code = code };
    }
}
