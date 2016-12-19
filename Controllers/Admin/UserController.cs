using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace Blog.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        //GET: User/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminUserNames(users, database);
                ViewBag.Admins = admins;

                return View(users);
            }
        }

        private HashSet<string> GetAdminUserNames(List<ApplicationUser> users, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }
            return admins;
        }

        //
        //GET User/Edit
        public ActionResult Edit(string id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            using (var database = new BlogDbContext())
            {
                var user = database.Users
                    .Where(u => u.Id == id)
                    .First();

                if (user == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);

                return View(viewModel);

            }

        }

        //
        //POST: User/Edit
        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var user = database.Users
                        .FirstOrDefault(u => u.Id == id);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHash;
                    }

                    user.Email = viewModel.User.Email;
                    user.FullName = viewModel.User.FullName;
                    user.UserName = viewModel.User.Email;
                    this.SetUserRoles(user, database, viewModel);

                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(viewModel);
        }

        //
        // GET: User/Delete
        public ActionResult Delete(string id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadGateway);
            }

            using (var database = new BlogDbContext())
            {
                var user = database.Users
                    .Where(u=>u.Id == id)
                    .First();

                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
        }
        //
        // POST: User/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user from DB
                var user = database.Users
                    .Where(u => u.Id == id)
                    .First();
                //Get user articles from DB
                var userArticles = database.Articles
                    .Where(a => a.Author.Id == user.Id);
                //Delete user articles
                foreach (var article in userArticles)
                {
                    database.Articles.Remove(article);
                }
                //Delete user and save changes
                database.Users.Remove(user);
                database.SaveChanges();

                return RedirectToAction("List");
            }
        }

        private IList<Role> GetUserRoles(ApplicationUser user, BlogDbContext db)
        {
            //create user man
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();
            //Get all app roles
            var roles = db.Roles
                .Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();
            // for each app role check if the user has it
            var userRoles = new List<Role>();
            foreach (var roleName in roles)
            {
                var role = new Role() {Name = roleName};

                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);
            }
            //return a list 
            return userRoles;
        }

        private void SetUserRoles(ApplicationUser user, BlogDbContext db, EditUserViewModel model)
        {
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

    }
}