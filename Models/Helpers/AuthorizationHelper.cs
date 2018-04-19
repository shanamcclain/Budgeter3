using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Budgeter3.Models.Helpers
{
    public class AuthorizationHelper : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (isAuthorized)
            {
                return false;
            }
            return httpContext.User.Identity.IsInHousehold();
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "CreateJoinHousehold"
                }));
            }
        }

        public class ValidateHouseholdId : AuthorizeAttribute
        {
            public int Param { get; set; }
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var authorized = base.AuthorizeCore(httpContext);
                if (!authorized)
                {
                    // the user is either not authenticated or not authorized
                    // no need to continue any further
                    return false;
                }

                // at this stage we know that the user is authenticated and
                // authorized (in roles), so let's go ahead and see who this 
                // user is
                string userId = httpContext.User.Identity.GetUserId();

                // now let's read the id. In this example I fetch it from
                // the route data but you could adapt according to your needs
                int id = Convert.ToInt32(httpContext.Request.RequestContext.RouteData.Values["id"]);

                var controller = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();


                // Now that we know the user and the id let's go ahead and 
                // check in our backend if the user is really the owner
                // of this id:
                return IsOwner(userId, id, controller);
            }

            private bool IsOwner(string userId, int id, string controller)
            {
                // go ahead and hit the backend   
                ApplicationDbContext db = new ApplicationDbContext();
                int usrHhId = db.Users.Find(userId).HouseholdId.Value;
                int urlId = 0;

                switch (controller)
                {
                    case "PersonalAccounts":
                        urlId = db.PersonalAccounts.Find(id).HouseholdId;
                        break;
                    case "Transactions":
                        urlId = db.Transactions.Find(id).Account.HouseholdId;
                        break;
                    case "Budgets":
                        urlId = db.Budgets.Find(id).HouseholdId;
                        break;
                    case "BudgetItems":
                        urlId = db.BudgetItems.Find(id).Budget.HouseholdId;
                        break;
                    case "Categories":
                        break;

                }
                bool result = (usrHhId == urlId);
                return result;
                //throw new NotImplementedException();
            }

            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    base.HandleUnauthorizedRequest(filterContext);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index"
                //id = UrlParameter.Optional
            }));
                }
            }


        }
    }
}