using NovelAI.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NovelAI
{
    public class NovelClient : NovelClientBase
    {
        public NovelClient(string baseUrl, HttpClient httpClient) : base(baseUrl, httpClient)
        {
        }

        public override async Task<SuccessfulLoginResponse> UserLoginAsync(LoginRequest body)
        {
            return await UserLoginAsync(body, System.Threading.CancellationToken.None);
        }

        public override async Task<SuccessfulLoginResponse> UserLoginAsync(LoginRequest body, CancellationToken cancellationToken)
        {
            var loginResponse = await base.UserLoginAsync(body, cancellationToken);
            SetToken(loginResponse.AccessToken);
            return loginResponse;
        }

        private void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
