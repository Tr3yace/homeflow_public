using System;
using System.Text.Json.Serialization;

namespace HomeFlow.Data.Models
{
    public class CouncilBinItem
    {
        [JsonPropertyName("hso_statesource")]
        public string HsoStateSource { get; set; } = string.Empty;

        [JsonPropertyName("hso_roundleg")]
        public string HsoRoundleg { get; set; } = string.Empty;

        [JsonPropertyName("hso_nextcollection")]
        public DateTimeOffset? HsoNextCollection { get; set; }

        [JsonPropertyName("hso_uprn")]
        public string HsoUprn { get; set; } = string.Empty;

        [JsonPropertyName("hso_reason")]
        public string HsoReason { get; set; } = string.Empty;

        [JsonPropertyName("hso_resolutionname")]
        public string HsoResolutionName { get; set; } = string.Empty;

        [JsonPropertyName("hso_subscriptionenddate")]
        public DateTimeOffset? HsoSubscriptionEndDate { get; set; }

        [JsonPropertyName("hso_resolutionsource")]
        public string HsoResolutionSource { get; set; } = string.Empty;

        [JsonPropertyName("hso_servicename")]
        public string HsoServiceName { get; set; } = string.Empty;

        [JsonPropertyName("hso_lastcollection")]
        public DateTimeOffset? HsoLastCollection { get; set; }

        [JsonPropertyName("hso_scheduledescription")]
        public string HsoScheduleDescription { get; set; } = string.Empty;

        [JsonPropertyName("hso_name")]
        public string HsoName { get; set; } = string.Empty;

        [JsonPropertyName("hso_nextcollectiondate")]
        public DateTimeOffset? HsoNextCollectionDate { get; set; }

        [JsonPropertyName("hso_missedcollectionallowed")]
        public bool HsoMissedCollectionAllowed { get; set; }

        [JsonPropertyName("hso_lastcollectiondate")]
        public DateTimeOffset? HsoLastCollectionDate { get; set; }

        [JsonPropertyName("hso_round")]
        public string HsoRound { get; set; } = string.Empty;

        [JsonPropertyName("hso_lastcollectioncompleted")]
        public DateTimeOffset? HsoLastCollectionCompleted { get; set; }

        [JsonPropertyName("hso_serviceid")]
        public int HsoServiceId { get; set; }

        [JsonPropertyName("hso_statename")]
        public string HsoStateName { get; set; } = string.Empty;

        [JsonPropertyName("hso_lastcollectioncompleteddate")]
        public DateTimeOffset? HsoLastCollectionCompletedDate { get; set; }

        [JsonPropertyName("hso_virtualserviceinformationid")]
        public Guid HsoVirtualServiceInformationId { get; set; }

        [JsonPropertyName("hso_roundgroup")]
        public string HsoRoundGroup { get; set; } = string.Empty;


    }
}
