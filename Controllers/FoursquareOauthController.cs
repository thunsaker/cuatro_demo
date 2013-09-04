using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Cuatro.Common;
using System.Web.Security;

namespace thunsaker.cuatro.demo.Controllers
{
    [HandleError]
    public class FoursquareOauthController : Controller
    {
        //
        // GET: /FoursquareOauth/Logon
        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOn()
        {
            string logonUri = String.Format("https://foursquare.com/oauth2/authenticate?client_id={0}&response_type=code&redirect_uri={1}", FoursquareSecrets.CLIENT_ID, FoursquareSecrets.REDIRECT_URI);
            return Redirect(logonUri);
        }

        //
        // GET: /FoursquareOauth/Receiver
        /// <summary>
        /// Receive the oauth token from Foursquare
        /// </summary>
        /// <returns></returns>
        public ActionResult Receiver()
        {
            Session["foursquare_access_token"] = "";
            string code = Request["code"] ?? "";
            var tokenJson = Request["access_token"] ?? null;
            string message = Request["error"] ?? "";
            FoursquareUser user = new FoursquareUser();

            if (tokenJson == null)
            {
                if (code != "" && code != null)
                {
                    StringBuilder accessTokenUri = new StringBuilder();
                    accessTokenUri.Append("https://foursquare.com/oauth2/access_token");
                    accessTokenUri.Append(String.Format("?client_id={0}", FoursquareSecrets.CLIENT_ID));
                    accessTokenUri.Append(String.Format("&client_secret={0}", FoursquareSecrets.CLIENT_SECRET));
                    accessTokenUri.Append("&grant_type=authorization_code");
                    accessTokenUri.Append(String.Format("&redirect_uri={0}", FoursquareSecrets.REDIRECT_URI));
                    accessTokenUri.Append("&code=" + code);

                    var response = WebRequestHelper.WebRequest(WebRequestHelper.Method.GET, accessTokenUri.ToString(), string.Empty);

                    if (response != null && response != "")
                    {
                        var json = JObject.Parse(response);
                        string token = json["access_token"].ToString().Replace("\"", "");

                        String foursquareUserUri = String.Format("https://api.foursquare.com/v2/users/self?oauth_token={0}", token);
                        var responseUser = WebRequestHelper.WebRequest(WebRequestHelper.Method.GET, foursquareUserUri.ToString(), string.Empty);
                        var jsonUser = JObject.Parse(responseUser);

                        if (int.Parse(jsonUser["meta"]["code"].ToString()) == 200)
                        {
                            user.AccessToken = token;
                            user.FirstName = jsonUser["response"]["user"]["firstName"].ToString().Replace("\"", "");
                            user.LastName = jsonUser["response"]["user"]["lastName"].ToString().Replace("\"", "");
                            user.FoursquareUserId = int.Parse(jsonUser["response"]["user"]["id"].ToString().Replace("\"", ""));
                            user.Gender = jsonUser["response"]["user"]["gender"].ToString().Replace("\"", "");
                            user.HomeCity = jsonUser["response"]["user"]["homeCity"].ToString().Replace("\"", "");
                            user.PhotoUri = jsonUser["response"]["user"]["photo"].ToString().Replace("\"", "");
                        }
                        else
                        {
                            var errorCode = int.Parse(jsonUser["meta"]["code"].ToString());
                            var errorMessage = jsonUser["meta"]["errorMessage"].ToString() ?? null;
                            var errorToSend = String.Format("Error code: {0} Error Message: {1}", errorCode, errorMessage);
                            return RedirectToAction("Error", new { error = errorToSend });
                        }
                    }
                }
                else
                {
                    ViewBag.Message = "Error: " + message;
                }
            }

            if (user.FoursquareUserId > 0)
            {
                Session["foursquare_access_token"] = user.AccessToken;
                //FormsAuthentication.SetAuthCookie(user.FoursquareUserId.ToString(), false);
                Session["CurrentUser"] = user;
                Session["foursquareId"] = user.FoursquareUserId;
                return RedirectToAction("Profile", "Home", user);
            }
            else
                return RedirectToAction("Error", "Home", new { error = "Problem Authenticating user" });
        }

        //
        // GET: /FoursquareOauth/LogOff
        /// <summary>
        /// Clear out the logged in user info from session
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            Session["foursquare_access_token"] = "";
            Session["CurrentUser"] = null;
            Session["foursquareId"] = 0;
            return RedirectToAction("Index", "Home");
        }
    }
}
