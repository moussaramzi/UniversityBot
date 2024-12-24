using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using CoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UniversityBot.DataServices;
using System.Threading.Tasks;

namespace CoreBot.Cards
{
    public class EnrollStudentCard
    {
        public static async Task<Attachment> CreateCardAttachmentAsync(int studentId, string courseTitle)
        {
            var student = await StudentDataService.GetStudentByIdAsync(studentId);
            var course = await CourseDataService.GetCoursesAsync();
            var selectedCourse = course.FirstOrDefault(c => c.Title.Equals(courseTitle, StringComparison.OrdinalIgnoreCase));

            if (student == null || selectedCourse == null)
            {
                throw new Exception("Student or Course not found.");
            }

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    // Title TextBlock
                    new AdaptiveTextBlock
                    {
                        Text = "Confirm Enrollment",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    // Student Name TextBlock
                    new AdaptiveTextBlock
                    {
                        Text = $"Student: {student.FirstName}",
                        Wrap = true
                    },
                    // Course Title TextBlock
                    new AdaptiveTextBlock
                    {
                        Text = $"Course: {selectedCourse.Title}",
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    // Confirm Action
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm",
                        Data = new { action = "confirmEnrollment", studentId = student.StudentID, courseTitle = selectedCourse.Title }
                    },
                    // Cancel Action
                    new AdaptiveSubmitAction
                    {
                        Title = "Cancel",
                        Data = new { action = "cancelEnrollment" }
                    }
                }
            };

            // Create an attachment
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };

            return adaptiveCardAttachment;
        }
    }
}
