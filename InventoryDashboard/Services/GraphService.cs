using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text.Json;


namespace InventoryDashboard.Services
{
    public class GraphService
    {
        private readonly GraphServiceClient _graphClient;

        public GraphService(IConfiguration configuration)
        {
            // Load values from appsettings.json
            var clientId = configuration["AzureAd:ClientId"];
            var tenantId = configuration["AzureAd:TenantId"];
            var clientSecret = configuration["AzureAd:ClientSecret"];

            // 1️⃣ Authenticate using Client Credential Flow
            var confidentialClient = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var authProvider = new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                var result = await confidentialClient
                    .AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                    .ExecuteAsync();

                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.AccessToken);
            });

            _graphClient = new GraphServiceClient(authProvider);
        }

        /// <summary>
        /// Reads data from a shared Excel file in OneDrive.
        /// </summary>
        public async Task<IList<IList<object>>> ReadExcelAsync(string fileId, string sheetName)
        {
            try
            {
                var docs = await _graphClient
                    .Users["{outlook_email}"]
                    .Drive
                    .Root
                    .Request()
                    .GetAsync();

                //foreach (var item in docs)
                //{
                //    Console.WriteLine(item.Name);
                //}

                var driveItem = await _graphClient
                    .Users["{outlook_email}"]   // or your correct user principal
                    .Drive
                    .Root
                    .ItemWithPath("Documents/InventoryData.xlsx")
                    .Request()
                    .GetAsync();

                string fileId1 = driveItem.Id;

                // 1️⃣ Get the used range from the worksheet
                var usedRange = await _graphClient.Users["{outlook_email}"]
                    .Drive
                    .Items["AB009120-6515-4A94-9C8A-5B3F2FDF78A8"]
                    .Workbook
                    .Worksheets[sheetName]
                    .UsedRange()
                    .Request()
                    .GetAsync();

                // 2️⃣ Check for null to avoid conversion errors
                if (usedRange == null || usedRange.Values == null)
                {
                    return new List<IList<object>>();
                }

                var parsedValues = ParseJsonElementToList(usedRange.Values);

                return parsedValues;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine("Error Code: " + ex.Error.Code);
                Console.WriteLine("Message: " + ex.Error.Message);
                Console.WriteLine("Inner Error: " + ex.Error.InnerError?.Message);
                throw;
            }
        }

        private IList<IList<object>> ParseJsonElementToList(object values)
        {
            var list = new List<IList<object>>();

            if (values is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in jsonElement.EnumerateArray())
                {
                    var rowList = new List<object>();
                    foreach (var cell in row.EnumerateArray())
                    {
                        switch (cell.ValueKind)
                        {
                            case JsonValueKind.String:
                                rowList.Add(cell.GetString());
                                break;
                            case JsonValueKind.Number:
                                if (cell.TryGetInt64(out var l))
                                    rowList.Add(l);
                                else if (cell.TryGetDouble(out var d))
                                    rowList.Add(d);
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                rowList.Add(cell.GetBoolean());
                                break;
                            case JsonValueKind.Null:
                                rowList.Add(null);
                                break;
                            default:
                                rowList.Add(cell.ToString());
                                break;
                        }
                    }
                    list.Add(rowList);
                }
            }

            return list;
        }

    }
}
