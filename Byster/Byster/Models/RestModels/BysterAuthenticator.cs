using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace Byster.Models.RestModels
{
    class BysterAuthenticator : IAuthenticator
    {
        public string sessionToken { get; set; }

        public BysterAuthenticator(string sessionToken)
        {
            this.sessionToken = sessionToken;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("Authorization", sessionToken);
        }
    }
}
