using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;

namespace MockSchoolManagement.DataRepositories
{
    public class SQLStudentRepository:IStudentRepository
    {
        private readonly AppDbContext _appDbContext;

        public SQLStudentRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
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
