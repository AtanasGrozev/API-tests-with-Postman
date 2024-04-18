using FooodyAPITests.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FooodyAPITests


{
    public class FoodyTests
    {
        private RestClient client;
        private static string foodId;



        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetAccessToken("atanas", "123456");

            var restOptions = new RestClientOptions("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86")
            {
                Authenticator = new JwtAuthenticator(accessToken)

            };

            this.client = new RestClient(restOptions);

        }

        private string GetAccessToken(string username, string password)
        {
            var authClient = new RestClient("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86");

            var authRequest = new RestRequest("/api/User/Authentication", Method.Post);

            authRequest.AddJsonBody(new AuthenticationRequest
            {
                UserName = username,
                Password = password

            });

            var response = authClient.Execute(authRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = JsonSerializer.Deserialize<Authentication_response>(response.Content);
                var accessToken = content.AccessToken;
                return accessToken;
            }
            else
            {
                throw new InvalidOperationException("Authentication failed");
            }
        }
        [Order(1)]
        [Test]

        public void CreateFood_WithCorrectData_ShouldSucceed()
        {
            var newFood = new FoodDTO
            {
                Name = "TestName",
                Description = "TestDescription",

            };

            var request = new RestRequest("/api/Food/Create", Method.Post);

            request.AddJsonBody(newFood);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            foodId = data.FoodId;

        }
        [Order(2)]
        [Test]
        public void EditTitle_WithNewTitle_ShouldSucceed()
        {
            var request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);

            request.AddJsonBody(new[]
                {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title"
                }

            });

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Successfully edited"));
        }
        [Order(3)]
        [Test]
        public void GetAllFoods_ShoudRetrunNotEmptyArray()
        {
            //Arrange
            var request = new RestRequest ("/api/Food/All" , Method.Get);


            //Act
            var response = this.client.Execute(request);

            //Assert    

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(content, Is.Not.Empty);

        }
        [Order(4)]
        [Test]
        public void DeleteEditedFoodWithCorrectId_ShoudDeleteFood()
        {
            //Arrange
            var request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);


            //Act
            var response = this.client.Execute(request);

            //Assert    

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Deleted successfully!"));
        }
        [Order(5)]
        [Test]

        public void CreateFood_WithInCorrectData_Shouldfaild()
        {
            var newFood = new FoodDTO
            {
                Name = "TestName",
                //Description = "TestDescription",

            };

            var request = new RestRequest("/api/Food/Create", Method.Post);

            request.AddJsonBody(newFood);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            foodId = data.FoodId;

        }
        [Order(6)]
        [Test]
        public void EditTitle_WithNoExistingFood_ShouldFAild()
        {
            string incorrectFoodId = "123131";
            var request = new RestRequest($"/api/Food/Edit/{incorrectFoodId}", Method.Patch);

            request.AddJsonBody(new[]
                {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title"
                }

            });

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

           Assert.That(content.Message, Is.EqualTo("No food revues..."));
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistigId_ShoudFail()
        {
            //Arrange
            string incorrectFoodId = "123131";
            var request = new RestRequest($"/api/Food/Delete/{incorrectFoodId}", Method.Delete);


            //Act
            var response = this.client.Execute(request);

            //Assert    

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Unable to delete this food revue!"));
        }
    }
}