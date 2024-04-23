using RestSharp.Authenticators;
using RestSharp;
using System.Net;
using System.Text.Json;
using StorySpoilAPItestProject.Models_DTO;

namespace StorySpoilAPItestProject
{
    public class Tests
    {

        private RestSharp.RestClient client;
        private const string BASEURL = "https://d5wfqm7y6yb3q.cloudfront.net";
        private const string USERNAME = "atanas";
        private const string PASSWORD = "123456";
        private static string storyId;

        [OneTimeSetUp]

        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient authClint = new RestClient(BASEURL);

            var request = new RestRequest("/api/User/Authentication");

            request.AddJsonBody(new
            {
                username,
                password
            });

            var response = authClint.Execute(request, Method.Post);
            if (response.StatusCode == HttpStatusCode.OK)
            {

                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }
                return token;

            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }

        }

        [Test, Order(1)]
        public void CreateNewStorySpoiler_WithCorrectData_ShoudSucceed()
        {
            var newStory = new StoryDTO()
            {
                Title = "New Story Titile ",
                Description = "New Story Description",
                Url = ""

            };

            var request = new RestRequest("/api/Story/Create").AddBody(newStory);
          

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.StoryId, Is.Not.Empty);
            Assert.That(responseData.StoryId, Is.Not.Null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
            storyId = responseData.StoryId;

        }    

        [Test, Order(2)]

        public void EditExistingStoryThatIsCreated_ShouldSucceed()
        {
           
            var editedStory = new StoryDTO
            {
                Title = "Edited Story",                
                Description = "Updated edited description.",
                Url = ""

            };
            var request = new RestRequest($"/api/Story/Edit/{storyId}", Method.Put);
          
            request.AddJsonBody(editedStory);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }
        [Test, Order(3)]

        public void DeleteStory_ShouldSucceed()
        {
            var request = new RestRequest($"/api/Story/Delete/{storyId}", Method.Delete);

      
            var response = this.client.Execute(request);
            var responeData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
            Assert.That(responeData.Msg, Is.EqualTo("Deleted successfully!"));

        }
        [Test, Order(4)]
        public void CreateNewStorySpoiler_WithMissingFileds_ShoudFail()
        {
            var newStory = new StoryDTO()
            {                
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create").AddBody(newStory);    

            var response = client.Execute(request, Method.Post);   

           

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));          


        }

        [Test, Order(5)]

        public void EditNonExistingStory_ShouldFail()
        {

            var editedStory = new StoryDTO
            {
                Title = "Edited Story",
                Description = "Updated edited description.",
                Url = ""
            };
            string nonExitingStoryId = "213124";
            var request = new RestRequest($"/api/Story/Edit/{nonExitingStoryId}", Method.Put);

            request.AddJsonBody(editedStory);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editResponse.Msg, Is.EqualTo("No spoilers..."));
        }
        [Test, Order(6)]

        public void DeleteNonExistingStory_ShouldFail()
        {
            string nonExitingStoryId = "213124";
            var request = new RestRequest($"/api/Story/Delete/{nonExitingStoryId}", Method.Delete);


            var response = this.client.Execute(request);
            var responeData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));        
            Assert.That(responeData.Msg, Is.EqualTo("Unable to delete this story spoiler!"));

        }

    }
}
