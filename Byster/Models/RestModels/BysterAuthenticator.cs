using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.RestModels
{
    class BysterAuthenticator : IAuthenticator
    {
        public string SessionToken { get; set; }

        public BysterAuthenticator(string sessionToken) => SessionToken = sessionToken;

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("Authorization", SessionToken);
        }
    }
}
