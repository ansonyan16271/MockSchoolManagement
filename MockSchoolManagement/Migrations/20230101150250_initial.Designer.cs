﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MockSchoolManagement.Infrastructure;

#nullable disable

namespace MockSchoolManagement.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230101150250_initial")]
    partial class initialDate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MockSchoolManagement.Models.Student", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Major")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhotoPath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Students");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "ansonyan16271@163.com",
                            Major = 1,
                            Name = "Anson"
                        },
                        new
                        {
                            Id = 2,
                            Email = "zhangsan@52abp.com",
                            Major = 2,
                            Name = "张三"
                        },
                        new
                        {
                            Id = 3,
                            Email = "lisi@52abp.com",
                            Major = 3,
                            Name = "李四"
                        },
                        new
                        {
                            Id = 4,
                            Email = "zhaoliu@52abp.com",
                            Major = 2,
                            Name = "赵六"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
