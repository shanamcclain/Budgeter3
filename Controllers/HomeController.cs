using Budgeter3.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Budgeter3.Controllers
{
    public class HomeController : Controller
    {
        [AuthorizationHelper]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            Contact model = new Contact();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(Email model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    EmailService ems = new EmailService();
                    IdentityMessage msg = new IdentityMessage();

                    msg.Body = "<p>Email From: <bold>" + model.FromName + "</bold> " + model.FromEmail + "</bold> " + model.Subject + "</p><p>Message:</p><p>" + model.Body + "</p>" + Environment.NewLine +
                        "<p>This is a message from your Budgeter Site.The name and Email of the contacting person is above.</p>";

                    msg.Destination = ConfigurationManager.AppSettings["emailto"];
                    msg.Subject = "Budgeter Contact Email";
                    await ems.SendMailAsync(msg);
                    TempData["BlogMessage"] = "Your Email has been sent";

                }
                catch (Exception Ex)
                {
                    //Console.WriteLine(Ex.Message);
                    await Task.FromResult(0);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult LandingPage()
        {
            return View();
        }
    }
}