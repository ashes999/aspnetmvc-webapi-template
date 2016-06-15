using NUnit.Framework;
using ProtonAnalytics.FunctionalTests.TestHelpers;
using ProtonAnalytics.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtonAnalytics.FunctionalTests.Controllers
{
    [TestFixture]
    class AccountControllerTests
    {
        // Workflow test
        [Test]
        public void UserCanRegisterLogInAndLogout()
        {
            var userName = "test@test.com";
            var password = "P@ssw0rd";

            // Delete the user if they already exists
            DatabaseFacade.ExecuteQuery("DELETE FROM AspNetUsers WHERE email = @email", new { email = userName });

            var client = new RestSharp.RestClient("http://localhost/ProtonAnalytics/api");

            // Register
            var request = new RestRequest("Account/Register", Method.POST);

            request.AddObject(new RegisterBindingModel()
            {
                Email = userName,
                Password = password,
                ConfirmPassword = password
            });

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, response.Content);

            // Log in
            request = new RestRequest("Account/Login", Method.POST);
            request.AddObject(new LoginBindingModel()
            {
                Email = userName,
                Password = password
            });

            response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Cookies.Count, Is.GreaterThan(0));

            // Log out
            request = new RestRequest("Account/LogOut", Method.POST);
            foreach (var cookie in response.Cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Cookies.Count, Is.EqualTo(0));
        }
    }
}
