using AdaptiveCards;
using CoreBot.DialogDetails;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class EnrollStudentCard
{
    public static Attachment CreateConfirmationCard(EnrollStudentDetails details)
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = "Enrollment Confirmation",
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Large,
                    Spacing = AdaptiveSpacing.Medium,
                    Wrap = true,
                },
                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact("First Name", details.FirstName ?? "Not provided"),
                        new AdaptiveFact("Last Name", details.LastName ?? "Not provided"),
                        new AdaptiveFact("Email", details.StudentMail ?? "Not provided"),
                        new AdaptiveFact("Courses", string.Join(", ", details.CourseTitles) ?? "No courses selected"),
                    }
                }
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = "Confirm",
                    Data = new { action = "confirmEnrollment" }
                },
                new AdaptiveSubmitAction
                {
                    Title = "Cancel",
                    Data = new { action = "cancelEnrollment" }
                }
            }
        };

        return new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JObject.FromObject(card)
        };
    }
}
