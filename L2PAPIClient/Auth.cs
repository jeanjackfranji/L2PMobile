using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using L2PAPIClient.DataModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace L2PAPIClient
{
    public class Auth
    {
        private UserConfig config = new UserConfig();

        public Auth() { }

        #region general Authentication Managemant

        public enum AuthenticationState
        {
            NONE = 0, // Not Authenticated, no Refresh Token known
            ACTIVE = 1, // Not necessarily authenticated at the moment, but can re-authenticate by getting an access Token using the refresh token or holding valid token at the moment
            WAITING = 2 // There is an ongoing Authentication process (e.g. a user needs to confirm authorization in the browser
        }

        /// <summary>
        /// Storing the representation of the actual authentication state - You may want to change that depending on your Device
        /// </summary>
        private AuthenticationState State = AuthenticationState.NONE;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public  AuthenticationState getState()
        {
            // Store the State independent from App Lifecycle for Mobile Apps e.g.            
            return State;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public  void setState(AuthenticationState authState)
        {
            State = authState;
        }

        /// <summary>
        /// Forces the Manager to refresh the State and tries to regenerate an accessToken if possible
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public  async Task CheckStateAsync()
        {
            if (getState() == AuthenticationState.WAITING)
            {
                // Authentication Process ongoing - do nothing and wait
                return;
            }

            if (config.getAccessToken() == "")
            {
                if (config.getRefreshToken() == "")
                {
                    // No access or refresh Token available!
                    setState(AuthenticationState.NONE);
                    return;
                }
                else
                {
                    // No Access Token, but refreshToken
                    await GenerateAccessTokenFromRefreshTokenAsync();
                    return;
                }
            }

            if (config.getRefreshToken() == "")
            {
                // No refreshtoken, but holding an Access Token - Check Token
                await CheckAccessTokenAsync();
                if (config.getAccessToken() == "")
                {
                    // Holding a valid AccesToken now
                    setState(AuthenticationState.ACTIVE);
                    return;
                }
                else
                {
                    // Validation failed - no tokens!
                    setState(AuthenticationState.NONE);
                }
            }

        }

        /// <summary>
        /// An Exception for Indicating problem with the authorization state
        /// </summary>
        public class NotAuthorizedException : Exception
        {
            public NotAuthorizedException(string text) : base(text) { }
            public NotAuthorizedException() : base() { }

        }

        private  Mutex CheckAccessTokenMutex = new Mutex();
        /// <summary>
        /// Checks the AccessToken against the tokenInfo REST-Service.
        /// If it fails, the system tries to refresh using the authentication manager
        /// If it still fails, accessToken is removed automatically
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public async  Task CheckAccessTokenAsync()
        {
            // use mutex for sync
            //CheckAccessTokenMutex.WaitOne();
            //string call = "{ \"client_id:\" \""+config.ClientID+"\" \"access_token\": \""+config.getAccessToken()+"\" }";

            //var answer = await RESTCalls.RestCallAsync<OAuthTokenInfo>("", config.OAuthTokenInfoEndPoint+"?accessToken="+config.getAccessToken()+"&client_id="+config.ClientID, true);
            var answer = await api.Calls.CheckValidTokenAsync();

            if (!answer)
            {
                // Try to refresh the token
                bool success = await GenerateAccessTokenFromRefreshTokenAsync();
                //call = "{ \"client_id:\" \"" + config.ClientID + "\" \"access_token\": \"" + config.getAccessToken() + "\" }";

                if (!success)
                {
                    // refreshToken and AccessToken are not working!
                    config.setAccessToken("");
                    config.setRefreshToken("");
                    setState(AuthenticationState.NONE);
                    // Inform caller!
                    throw new NotAuthorizedException("App not authorized");
                }

                answer = await api.Calls.CheckValidTokenAsync();
                if (!answer)
                {
                    // Invalid Token, no refreshToken success  - delete it
                    config.setAccessToken("");
                }
                else
                {
                    // Successful reconstructed Tokens
                    State = AuthenticationState.ACTIVE;
                }
            }
            else
            {
                State = AuthenticationState.ACTIVE;
            }
            //CheckAccessTokenMutex.ReleaseMutex();
        }

        # endregion

        # region Authorization Process

        private  Mutex StartAuthMutex = new Mutex();

        /// <summary>
        /// Starts the Autehntication Process
        /// </summary>
        /// <returns>returns the verification URL for this app or an empty string on fails</returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public async  Task<string> StartAuthenticationProcessAsync()
        {
            /*bool hasMutex = StartAuthMutex.WaitOne(5000);
            if (!hasMutex)
            {
                return "";
            }*/
            var answer = await OAuthInitCallAsync();
            if (answer.status == "ok")
            {
                // call was successfull!
                string url = answer.verification_url + "?q=verify&d=" + answer.user_code;
                config.setDeviceToken(answer.device_code);
                setState(AuthenticationState.WAITING);
                expireTimeWaitingProcess = answer.expires_in;
                Thread t = new Thread(new ThreadStart(ExpireThread));
                t.Start();
                //StartAuthMutex.ReleaseMutex();
                return url;
            }
            // Failed
            //StartAuthMutex.ReleaseMutex();
            return "";
        }

        private  int expireTimeWaitingProcess;

        private  Mutex CheckAuthMutex = new Mutex();

        /// <summary>
        /// Checks whether the users has already authenticated the app (Part of the Authentication process!)
        /// </summary>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public async  Task<OAuthTokenRequestData> CheckAuthenticationProgressAsync()
        {
            //CheckAuthMutex.WaitOne();
            var answer = await TokenCallAsync();
            if (answer == null || answer.status == null || answer.status.StartsWith("Fail:") || answer.status.StartsWith("error:"))
            {
                // Not working!
                return null;
            }
            // working!
            // Store the tokens
            config.setAccessToken(answer.access_token);
            config.setRefreshToken(answer.refresh_token);
            setState(AuthenticationState.ACTIVE);
            //CheckAuthMutex.ReleaseMutex();
            return answer;
        }

        private  Mutex InitAuthMutex = new Mutex();

        /// <summary>
        /// Initiates the Authorization process
        /// </summary>
        /// <returns>The answer on the Initial Call to Endpoint or an empty answer if something went wrong! (having a status field Fail: (Error message) )</returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        internal async  Task<OAuthRequestData> OAuthInitCallAsync()
        {
            //InitAuthMutex.WaitOne();
            string parsedContent = "{ \"client_id\": \"" + config.ClientID + "\", \"scope\": \"l2p.rwth campus.rwth l2p2013.rwth\" }";
            var answer = await api.Calls.RestCallAsync<OAuthRequestData>(parsedContent, config.OAuthEndPoint, true);
            //InitAuthMutex.ReleaseMutex();
            return answer;
        }


        private  Mutex TokenCallMutex = new Mutex();

        /// <summary>
        /// Calls the /token Endpoint to check status of authorization process
        /// </summary>
        /// <returns>The answer to the Call or an artificial answer containing an Error Descripstion in the status-field</returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        private async  Task<OAuthTokenRequestData> TokenCallAsync()
        {
            //TokenCallMutex.WaitOne();
            try
            {
                var req = new OAuthTokenRequestSendData();
                req.client_id = config.ClientID;
                req.code = config.getDeviceToken();
                req.grant_type = "device";

                string postData = JsonConvert.SerializeObject(req);

                var answer = await api.Calls.RestCallAsync<OAuthTokenRequestData>(postData, config.OAuthTokenEndPoint, true);
                //TokenCallMutex.ReleaseMutex();
                return answer;
            }
            catch (Exception e)
            {
                var t = e.Message;
                var pseudo = new OAuthTokenRequestData();
                pseudo.status = "Fail: " + t;
                //TokenCallMutex.ReleaseMutex();
                return pseudo;
            }
        }


        /// <summary>
        /// A sinple method that will reset the State to NONE after (expireTime) seconds
        /// </summary>
        private  void ExpireThread()
        {
            // Wait for the expire time
            Thread.Sleep(expireTimeWaitingProcess * 1000);
            // wake up and check, whether the process has expired
            setState(AuthenticationState.NONE);
        }

        # endregion


        private  Mutex RefreshhMutex = new Mutex();

        /// <summary>
        /// Uses the current RefreshToken to get an Access Token
        /// </summary>
        /// <returns>true, if the call was successfull</returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public async  Task<bool> GenerateAccessTokenFromRefreshTokenAsync()
        {
            //RefreshhMutex.WaitOne();
            string callBody = "{ \"client_id\": \"" + config.ClientID + "\", \"refresh_token\": \"" + config.getRefreshToken() + "\", \"grant_type\":\"refresh_token\" }";
            var answer = await api.Calls.RestCallAsync<OAuthTokenRequestData>(callBody, config.OAuthTokenEndPoint, true);
            if ((answer.error == null) && (!answer.status.StartsWith("error")) && (answer.expires_in > 0))
            {
                //Console.WriteLine("Got a new Token!");
                config.setAccessToken(answer.access_token);
                TokenExpireTime = answer.expires_in;
                //TokenExpireTime = 10;
                Refresher = new Thread(new ThreadStart(TokenRefresherThread));
                Refresher.Start();
                //RefreshhMutex.ReleaseMutex();
                return true;
            }
            // Failed!
            //RefreshhMutex.ReleaseMutex();
            return false;
        }

        private  Thread Refresher = null;
        private  int TokenExpireTime;

        private async  void TokenRefresherThread()
        {
            //Console.WriteLine("Startet Refresh-Thread");
            Thread.Sleep(TokenExpireTime * 1000);
            //Console.WriteLine("Refreshing!");
            await GenerateAccessTokenFromRefreshTokenAsync();
        }
    }
}

/*
 * This Application uses the Newtonsoft JSON package. This packages is licensed under MIT-License:
  
 The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
