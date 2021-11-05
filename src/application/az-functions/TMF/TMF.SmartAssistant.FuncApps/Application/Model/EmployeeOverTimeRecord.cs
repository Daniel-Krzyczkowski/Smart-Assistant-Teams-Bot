using System.Text.Json.Serialization;

namespace TMF.SmartAssistant.FuncApps.Application.Model
{
    internal class EmployeeOverTimeRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("employeeId")]
        public string EmployeeId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("totalWorkingHoursPerWeek")]
        public double TotalWorkingHoursPerWeek { get; set; }
    }
}
