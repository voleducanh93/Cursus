using PayPalCheckoutSdk.Core;

using Microsoft.Extensions.Options;
using Cursus.Common.Helper;

namespace Demo_PayPal.Service
{
    public class PayPalClient
    {
        private readonly PayPalSetting _payPalSettings;

        public PayPalClient(IOptions<PayPalSetting> payPalSettings)
        {
            _payPalSettings = payPalSettings.Value;
        }  
        
        public PayPalHttpClient Client()
        {      
            var environment = new SandboxEnvironment(_payPalSettings.ClientId, _payPalSettings.ClientSecret);
            return new PayPalHttpClient(environment);
        }
    }
}
