using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;

namespace MockSchoolManagement.DataRepositories
{
    public class SQLStudentRepository:IStudentRepository
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _appDbContext;

        public SQLStudentRepository(AppDbContext appDbContext, ILogger<SQLStudentRepository> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public Student Delete(int id)
        {
            Student student = _appDbContext.Students.Find(id);
            if(student != null) 
            {
                _appDbContext.Students.Remove(student);
                _appDbContext.SaveChanges();
            }
            return student;
        }

        public IEnumerable<Student> GetAllStudents()
        {
            _logger.LogTrace("学生信息 Trace(跟踪) Log");
            _logger.LogDebug("学生信息 Debug(调试) Log");
            _logger.LogInformation("学生信息 信息(Information) Log");
            _logger.LogWarning("学生信息 警告(Warning) Log");
            _logger.LogError("学生信息 错误(Error) Log");
            _logger.LogCritical("学生信息 严重(Critical) Log");

            return _appDbContext.Students;
        }

        public Student GetStudentById(int id)
        {
            return _appDbContext.Students.Find(id);
        }

        public Student Insert(Student student)
        {
            _appDbContext.Students.Add(student);
            _appDbContext.SaveChanges();
            return student;
        }

        public Student Update(Student updateStudent) 
        {
            var student=_appDbContext.Students.Attach(updateStudent);
            student.State=Microsoft.EntityFrameworkCore.EntityState.Modified;
            _appDbContext.SaveChanges();
            return updateStudent;
        }
    }
}
