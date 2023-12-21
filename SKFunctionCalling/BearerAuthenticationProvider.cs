using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FunctionCalling
{
    public class BearerAuthenticationProvider
    {
        private readonly Func<Task<string>> _bearerToken;

        /// <summary>
        /// Creates an instance of the <see cref="BearerAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="bearerToken">Delegate to retrieve the bearer token.</param>
        public BearerAuthenticationProvider(Func<Task<string>> bearerToken)
        {
            this._bearerToken = bearerToken;
        }

        /// <summary>
        /// Applies the token to the provided HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await this._bearerToken().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await this._bearerToken().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

}
