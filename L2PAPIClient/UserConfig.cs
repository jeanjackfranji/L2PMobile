using System;
using System.Collections.Generic;
using System.Text;

namespace L2PAPIClient
{
    public class UserConfig
    {
        public UserConfig() { }

        public string OAuthEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/code";

        public string OAuthTokenEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/token";

        public string ClientID = "TAgzsUUaRiAqRQAR8BSSvxmfPCITYeiSfAVuX8GDQIQHbKiFFWYkLQhEEBTwbS8q.apps.rwth-aachen.de";

        public string L2PEndPoint = "https://www3.elearning.rwth-aachen.de/_vti_bin/l2pservices/api.svc/v1";

        public string OAuthTokenInfoEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/tokeninfo";

        #region Token Management (Add Storage Option in here!)

        private string accessToken = "";

        public string getAccessToken()
        {
            return accessToken;
        }

        public void setAccessToken(string token)
        {
            accessToken = token;
        }

        private string refreshToken = "";

        public string getRefreshToken()
        {
            return refreshToken;
        }

        public void setRefreshToken(string token)
        {
            refreshToken = token;
        }


        private string deviceToken = "";

        public string getDeviceToken()
        {
            return deviceToken;
        }

        public void setDeviceToken(string token)
        {
            deviceToken = token;
        }


        #endregion
    }
}
