using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Options;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using MyProject.Shared.ViewModels;
using MyProject.Settings;

namespace MyProject.Function
{
    public class CheckUserExistence
    {
        private readonly AdminConfiguration adminSettings;

        public CheckUserExistence(IOptions<AdminConfiguration> adminSettings)
        {
            this.adminSettings = adminSettings.Value;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [FunctionName("CheckUserExistence")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var adUser = JsonConvert.DeserializeObject<AdUserViewModel>(requestBody);

            // If input data is null, show block page
            if (adUser == null)
            {
                return new OkObjectResult(new ResponseContent("ShowBlockPage", "There was a problem with your request."));
            }

            string tenantId = adminSettings.TenantId;
            string applicationId = adminSettings.ApplicationId;
            string clientSecret = adminSettings.ClientSecret;

            // If some configuration is missing, show block page
            if (string.IsNullOrEmpty(tenantId) ||
                string.IsNullOrEmpty(applicationId) ||
                string.IsNullOrEmpty(clientSecret))
            {
                return new OkObjectResult(new ResponseContent("ShowBlockPage", "There was a problem with your request."));
            }

            // If email claim not found, show block page
            if (string.IsNullOrEmpty(adUser.Email) || !adUser.Email.Contains("@"))
            {
                return new BadRequestObjectResult(new ResponseContent("ShowBlockPage", "Email name is mandatory."));
            }

            string userEmail = adUser.Email;

            // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(applicationId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            try
            {
                // Get user by sign-in name
                var result = (await graphClient.Users
                    .Request()
                    .Filter($"identities/any(c:c/issuerAssignedId eq '{userEmail}' and c/issuer eq '{tenantId}')")
                    .GetAsync())
                    .Union(
                        await graphClient.Users
                        .Request()
                        .Filter($"otherMails/any(c:c eq '{userEmail}') and UserType eq 'Member'")
                        .GetAsync()
                    ).ToArray();

                if (result.Length > 0)
                {
                    return new BadRequestObjectResult(new ResponseContent("ValidationError", "An user with this email already exists.", "400"));
                }
            }
            catch (Exception e)
            {
                log.LogError("Error executing MS Graph request: ", e);
                return new OkObjectResult(new ResponseContent("ShowBlockPage", "There was a problem with your request."));
            }

            // If all is OK, return 200 OK - Continue message
            return new OkObjectResult(new ResponseContent("Continue"));
        }
    }
}
