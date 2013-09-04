using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cuatro.Common;
using Cuatro.Common.Endpoints;
using thunsaker.cuatro.demo.Models;

namespace thunsaker.cuatro.demo.Controllers {
    public class FoursquareController : Controller {
        //
        // GET: /Foursquare/
        public ActionResult Index() {
            return View();
        }

        //
        // GET: /Foursquare/Badges
        public ActionResult Badges() {
            if (Session["foursquare_access_token"] != null) {
                string accessToken = Session["foursquare_access_token"].ToString().Replace("\"", "");

                Users myUser = new Users(null, accessToken);
                List<UnlockedBadge> myUserBadges = myUser.GetBadges();
                if (myUserBadges != null && myUserBadges.Count > 0)
                    return View(myUserBadges);
                else
                    return View();
            }

            return RedirectToAction("LogOn", "FoursquareOauth");
        }

        //
        // GET: /Foursquare/Requests
        public ActionResult Requests() {
            if (Session["CurrentUser"] != null) {
                FoursquareUser currentUser = Session["CurrentUser"] as Cuatro.Common.FoursquareUser;
                string accessToken = currentUser.AccessToken;

                Users myUser = new Users(currentUser, accessToken);
                List<FriendRequest> myRequests = myUser.GetFriendRequests();

                if (myRequests != null && myRequests.Count > 0)
                    return View(myRequests);
                else
                    return View();
            }

            return RedirectToAction("LogOn", "FoursquareOauth");
        }

        //
        // GET: /Foursquare/Leaderboard
        public ActionResult Leaderboard() {
            if (Session["CurrentUser"] != null) {
                FoursquareUser currentUser = Session["CurrentUser"] as Cuatro.Common.FoursquareUser;
                string accessToken = currentUser.AccessToken;

                Users myUser = new Users(currentUser, accessToken);
                Cuatro.Common.Leaderboard myLeaderboard = myUser.GetLeaderboard();

                if (myLeaderboard != null && myLeaderboard.UsersList != null && myLeaderboard.UsersList.Count > 0)
                    return View(myLeaderboard);
                else
                    return View();
            }

            return RedirectToAction("LogOn", "FoursquareOauth");
        }

        //
        // GET: /Foursquare/UserSearch
        public ActionResult UserSearch() {
            return View();
        }

        //
        // POST: /Foursquare/UserSearch
        [HttpPost]
        public ActionResult UserSearchSubmit(FormCollection collection) {
            if (Session["CurrentUser"] != null) {
                FoursquareUser currentUser = Session["CurrentUser"] as Cuatro.Common.FoursquareUser;
                string accessToken = currentUser.AccessToken;

                Users myUser = new Users(currentUser, accessToken);
                UserSearchType type = UserSearchType.twitter;
                // TODO: Consider creating its own class (UserSearchResult or something) to order by relevance/etc
                List<FoursquareUser> resultList = new List<FoursquareUser>();
                switch (int.Parse(collection.GetValue("typeIndex").ToString())) {
                    case 1:
                        type = UserSearchType.phone;
                        break;
                    case 2:
                        type = UserSearchType.email;
                        break;
                    case 3:
                        type = UserSearchType.fbid;
                        break;
                    case 4:
                        type = UserSearchType.twitter;
                        break;
                }

                resultList = myUser.SearchUser(type, collection.GetValue("query").ToString());

                if (resultList != null)
                    return PartialView("SearchResults", resultList);
                else
                    return PartialView();
            }

            return RedirectToAction("LogOn", "FoursquareOauth");
        }

        //
        // GET: /Foursquare/Checkins
        public ActionResult Checkins() {
            if (Session["CurrentUser"] != null) {
                var daysRequest = Request["days"] ?? null;
                int days = 0;
                FoursquareUser currentUser = Session["CurrentUser"] as Cuatro.Common.FoursquareUser;
                string accessToken = currentUser.AccessToken;

                Users myUser = new Users(currentUser, accessToken);
                if (daysRequest != null) {
                    days = int.Parse(daysRequest);
                    if (days > 365)
                        days = 365;
                } else
                    days = 7;

                ViewBag.Days = days;
                List<Cuatro.Common.Checkin> myCheckins = new List<Checkin>();
                List<Cuatro.Common.Checkin> tempCheckins = myUser.GetCheckins(new TimeSpan(Math.Abs(days), 0, 0, 0), 0);
                int theOffset = 0;

                myCheckins.AddRange(tempCheckins);

                if (tempCheckins.Count == 20) {
                    while (tempCheckins.Count == 20) {
                        tempCheckins = myUser.GetCheckins(new TimeSpan(Math.Abs(days), 0, 0, 0), theOffset);
                        if (tempCheckins != null) {
                            myCheckins.AddRange(tempCheckins);
                            theOffset += 20;
                        } else
                            break;
                    }
                }

                if (myCheckins != null && myCheckins.Count > 0)
                    return View(myCheckins);
                else
                    return View();
            }

            return RedirectToAction("LogOn", "FoursquareOauth");
        }

        [HttpGet]
        public ActionResult VenueSearch() {
            return View();
        }

        [HttpPost]
        public ActionResult VenueSearch(FormCollection collection) {
            try {
                List<Venue> results = new List<Venue>();

                string query = collection["SearchQuery"] ?? "";
                string near = collection["Near"] ?? "";
                double mylat = 0.00f;
                double mylong = 0.00f;
                double.TryParse(collection["Latitude"], out mylat);
                double.TryParse(collection["Longitude"], out mylong);
                LocationCoords coords = new LocationCoords() {
                    Latitude = mylat,
                    Longitude = mylong
                };
                
                FoursquareUser currentUser = Session["CurrentUser"] as Cuatro.Common.FoursquareUser;
                string accessToken = currentUser.AccessToken;

                Venues myVenues = new Venues(currentUser, accessToken);
                
                if(mylat != 0.00f && mylong != 0.00f) {
                    results = myVenues.SearchVenues(query, coords, near);
                }

                return View(new VenueSearchModel() {
                    Latitude = mylat,
                    Longitude = mylong,
                    Near = near,
                    SearchQuery = query,
                    VenueResults = results
                });
            } catch (Exception ex) {
                return RedirectToAction("Error", "Home", new { error = "There was a problem. Ex: " + ex.Message });
                throw;
            }
        }
    }
}