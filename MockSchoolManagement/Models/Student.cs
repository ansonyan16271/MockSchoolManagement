using MockSchoolManagement.Models.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace MockSchoolManagement.Models
{
    /// <summary>
    /// 学生模型
    /// </summary>
    public class Student
    {
        public int Id { get; set; }
        
        public string? Name { get; set; }
        
        public MajorEnum? Major { get; set; }
        
        public string Email { get; set; }
        public string? PhotoPath { get; set; }
    }
}
