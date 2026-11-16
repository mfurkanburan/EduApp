using EduApp.Web.Enums;

namespace EduApp.Web.Extensions;

public static class CourseLevelExtensions
{
    public static string ToDisplayName(this CourseLevel level)
    {
        return level switch
        {
            CourseLevel.Beginner => "Başlangıç",
            CourseLevel.Intermediate => "Orta",
            CourseLevel.Advanced => "İleri",
            _ => "Bilinmiyor"
        };
    }
}