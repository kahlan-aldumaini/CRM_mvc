using Vonage;
using Vonage.Messaging;
using Vonage.Request;

namespace CRM_mvc.Utilities.Config;

public class SMS
{
    private readonly string vonageApiKey;
    private readonly string vonageApiSecret;

    public SMS()
    {
        this.vonageApiKey = "3d0d771e";
        this.vonageApiSecret = "sqEqpBWCOXNw2aJA";
    }

    private VonageClient? Initial()
    {
        var credentials = Credentials.FromApiKeyAndSecret(
            vonageApiKey,
            vonageApiSecret
        );

        return new VonageClient(credentials);
    }
    
    public async Task<SendSmsResponse> Send(string to, string text,string from)
    {
        var vonageClient = Initial();
        
        return await vonageClient.SmsClient.SendAnSmsAsync(new SendSmsRequest()
        {
            To = to,
            From = from,
            Text = text
        });
    }
}