using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Budgeter3.Models;
using Budgeter3.Models.Helpers;
using Microsoft.AspNet.Identity;

namespace Budgeter3.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Households
        public ActionResult Index()
        {
            return View(db.Households.ToList());
        }

        // GET: Households/Details/5
        public ActionResult Details(int? id)
        {
            HouseholdViewModel vm = new HouseholdViewModel();
            var userid = User.Identity.GetUserId();
            var household = db.Users.Find(userid).Household;
            vm.HHId = household.Id;
            vm.HHName = household.Name;
            vm.Users = household.Members;
      
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (vm == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        // GET: Households/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Households.Add(household);
                db.SaveChanges();
                return RedirectToAction("Details");
            }

            return View(household);
        }

        // GET: Households/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        // GET: Households/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            db.Households.Remove(household);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public ActionResult CreateJoinHousehold(Guid? code)
        {
            //If the current user accessing this page already has a HouseholdId, send them to their dashboard
            if (User.Identity.IsInHousehold())
            {
                return RedirectToAction("Details", "Households", new { id = User.Identity.GetHouseholdId() });
            }

            HouseholdViewModel vm = new HouseholdViewModel();

            //Determine whether the user has been sent an invite and set property values 
            if (code != null)
            {
                string msg = "";
                if (ValidInvite(code, ref msg))
                {
                    Invite result = db.Invites.FirstOrDefault(i => i.HHToken == code);

                    vm.IsJoinHouse = true;
                    vm.HHId = result.HouseholdId;
                    vm.HHName = result.Household.Name;

                    //Set USED flag to true for this invite

                    result.HasBeenUsed = true;

                    ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
                    user.InviteEmail = result.Email;
                    db.SaveChanges();
                }
                else
                {
                    return RedirectToAction("InviteError", new { errMsg = msg });
                }
            }
            return View(vm);
        }

        private bool ValidInvite(Guid? code, ref string message)
        {

            if ((DateTime.Now - db.Invites.FirstOrDefault(i => i.HHToken == code).InviteDate).TotalDays < 6)
            {
                bool result = db.Invites.FirstOrDefault(i => i.HHToken == code).HasBeenUsed;
                if (result)
                {
                    message = "invalid";
                }
                else
                {
                    message = "valid";
                }

                return !result;
            }
            else
            {
                message = "expired";
                return false;
            }

        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateHousehold(HouseholdViewModel vm)
        {
            //Create new Household and save to DB
            Household hh = new Household();
            hh.Name = vm.HHName;
            db.Households.Add(hh);
            db.SaveChanges();

            //Add the current user as the first member of the new household
            var user = db.Users.Find(User.Identity.GetUserId());
            hh.Members.Add(user);
            db.SaveChanges();

            //Soution1
            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            //Solution2
            //((ClaimsIdentity)identity).RemoveClaim(identity.FindFirst(ClaimTypes.Name));
            //((ClaimsIdentity)identity).AddClaim(new Claim(ClaimTypes.Name, "new_name"));

            //Solution3
            //Task SignInManager<>.RefreshSignInAsync(vm.Member);

            //Solution4
            //identity.AddClaim(new Claim("myClaimType", "myClaimValue"));

            //var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            //authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);

            //Solution5 **BEST**
            await ControllerContext.HttpContext.RefreshAuthentication(user);

            return RedirectToAction("Details", "Households", new { id = User.Identity.GetHouseholdId() });
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> JoinHousehold(Household household)
        {
            Household hh = db.Households.Find(household.Id);
            var user = db.Users.Find(User.Identity.GetUserId());

            hh.Members.Add(user);
            db.SaveChanges();

            await ControllerContext.HttpContext.RefreshAuthentication(user);

            return RedirectToAction("Details", "Households", new { id = User.Identity.GetHouseholdId() });
        }
    }
}
