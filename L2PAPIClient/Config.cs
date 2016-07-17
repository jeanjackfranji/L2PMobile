using System;
using System.Collections.Generic;
using System.Text;

namespace L2PAPIClient
{
    public class Config
    {
	internal const string OAuthEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/code";

        internal const string OAuthTokenEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/token";

        internal const string ClientID = "TAgzsUUaRiAqRQAR8BSSvxmfPCITYeiSfAVuX8GDQIQHbKiFFWYkLQhEEBTwbS8q.apps.rwth-aachen.de";

        public const string L2PEndPoint = "https://www3.elearning.rwth-aachen.de/_vti_bin/l2pservices/api.svc/v1";

        internal const string OAuthTokenInfoEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/tokeninfo";

        #region Token Management (Add Storage Option in here!)

        private static string accessToken = "";

        public static string getAccessToken()
        {
            return accessToken;
        }

        public static void setAccessToken(string token)
        {
            accessToken = token;
        }

        private static string refreshToken = "";

        public static string getRefreshToken()
        {
            return refreshToken;
        }

        public static void setRefreshToken(string token)
        {
            refreshToken = token;
        }


        private static string deviceToken = "";

        internal static string getDeviceToken()
        {
            return deviceToken;
        }

        internal static void setDeviceToken(string token)
        {
            deviceToken = token;
        }


        #endregion
    }
}
