using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Models;
using MockSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockSchoolManagement.Infrastructure
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    Name = "Anson",
                    Major = MajorEnum.ComputerScience,
                    Email = "ansonyan16271@163.com",
                    
                });
            modelBuilder.Entity<Student>().HasData(
                 new Student
                 {
                     Id = 2,
                     Name = "张三",
                     Major = MajorEnum.ElectronicCommerce,
                     Email = "zhangsan@52abp.com",
                     
                 }
             );
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 3,
                    Name = "李四",
                    Major = MajorEnum.Mathematics,
                    Email = "lisi@52abp.com",
                    
                }
            );
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 4,
                    Name = "赵六",
                    Major = MajorEnum.ElectronicCommerce,
                    Email = "zhaoliu@52abp.com",
                    
                });
        }
    }
}
