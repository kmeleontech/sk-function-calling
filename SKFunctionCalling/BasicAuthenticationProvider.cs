using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SKFunctionCalling
{
    /// <summary>
    /// Retrieves authentication content (e.g. username/password, API key) via the provided delegate and
    /// applies it to HTTP requests using the "basic" authentication scheme.
    /// </summary>
    public class BasicAuthenticationProvider
    {
        private readonly Func<Task<string>> _credentials;

        /// <summary>
        /// Creates an instance of the <see cref="BasicAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="credentials">Delegate for retrieving credentials.</param>
        public BasicAuthenticationProvider(Func<Task<string>> credentials)
        {
            this._credentials = credentials;
        }

        /// <summary>
        /// Applies the authentication content to the provided HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Base64 encode
            string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(await this._credentials().ConfigureAwait(false)));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedContent);
        }

        /// <summary>
        /// Applies the authentication content to the provided HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Base64 encode
            string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(await this._credentials().ConfigureAwait(false)));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedContent);
        }
    }
}
