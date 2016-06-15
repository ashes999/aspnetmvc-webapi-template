using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Moq;
using NUnit.Framework;
using ProtonAnalytics.Controllers;
using ProtonAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProtonAnalytics.UnitTests.Controllers
{
    [TestFixture]
    class AccountControllerTests
    {
        [Test]
        public void LogOutLogsOutOfAuthenticationProvider()
        {
            var authManager = new Mock<IAuthenticationManager>();
            authManager.Setup(a => a.SignOut(CookieAuthenticationDefaults.AuthenticationType));

            var controller = new AccountController(null, null, authManager.Object, null);
            var view = controller.LogOut();
            Assert.IsTrue(view is System.Web.Http.Results.OkResult);
            authManager.VerifyAll();
        }

        [Test]
        public void LoginSignsInToSignInManager()
        {
            string userName = "test@test.com";
            string password = "P@ssw0rd";

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);
            signInManager.Setup(s => s.PasswordSignInAsync(userName, password, false, false)).ReturnsAsync(Microsoft.AspNet.Identity.Owin.SignInStatus.Success);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var result = controller.Login(new Models.LoginBindingModel() { Email = userName, Password = password });
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.OkResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }

        [TestCase("e@e.com", null)]
        [TestCase("", "P@ssw0rd")]
        public void LoginReturnsBadRequestIfModelIsInvalid(string email, string password)
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var model = new Models.LoginBindingModel()
            {
                Email = email,
                Password = password
            };
            
            var result = controller.Login(model);
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.InvalidModelStateResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }

        [Test]
        public void LoginReturnsBadRequestIfUserNameOrPasswordIsIncorrect()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);
            
            // Always fail regardless of credentials
            signInManager.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).ReturnsAsync(Microsoft.AspNet.Identity.Owin.SignInStatus.Failure);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var result = controller.Login(new Models.LoginBindingModel() { Email = "test@test.com", Password = "P@ssw0rd" });
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.InvalidModelStateResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }

        [Test]
        public void RegisterCallsCreateAsyncOnUserManager()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var model = new Models.RegisterBindingModel()
            {
                Email = "test@test.com",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd"
            };

            userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, password) =>
            {
                Assert.AreEqual(model.Email, user.Email);
                Assert.AreEqual(model.Password, password);
            })
            .ReturnsAsync(IdentityResult.Success);

            var result = controller.Register(model);
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.OkResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }

        [TestCase("hi@hi.com", "", "forgot my password")]
        [TestCase("hi@hi.com", "only password without confirmation", "")]
        [TestCase("", "password", "password")]
        public void RegisterReturnsBadResultIfRequestIsInvalid(string userName, string password, string confirmPassword)
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var model = new Models.RegisterBindingModel()
            {
                Email = userName,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var result = controller.Register(model);
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.InvalidModelStateResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }

        [Test]
        public void RegisterReturnsBadResultIfUserManagerReturnsFailure()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authManager.Object);

            var controller = new AccountController(userManager.Object, null, null, signInManager.Object);
            var model = new Models.RegisterBindingModel()
            {
                Email = "test@test.com",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd"
            };

            userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    Assert.AreEqual(model.Email, user.Email);
                    Assert.AreEqual(model.Password, password);
                })
            .ReturnsAsync(new IdentityResult("Invalid DB access"));

            var result = controller.Register(model);
            result.Wait();
            var view = result.Result;

            Assert.That(view is System.Web.Http.Results.InvalidModelStateResult);

            userStore.VerifyAll();
            userManager.VerifyAll();
            authManager.VerifyAll();
            signInManager.VerifyAll();
        }
    }
}
