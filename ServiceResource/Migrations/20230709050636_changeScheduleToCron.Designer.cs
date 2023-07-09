﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServiceResource.Persistence.Queue.Context;

#nullable disable

namespace ServiceResource.Migrations
{
    [DbContext(typeof(QueueContext))]
    [Migration("20230709050636_changeScheduleToCron")]
    partial class changeScheduleToCron
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ServiceResource.Persistence.Queue.Entities.QueueSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CallBackAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CallBackIntervalCronSchedule")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CallBackMaxCallCount")
                        .HasColumnType("int");

                    b.Property<int>("CallBackMaxCallsPerInterval")
                        .HasColumnType("int");

                    b.Property<string>("IntervalCronSchedule")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaxCallCount")
                        .HasColumnType("int");

                    b.Property<int>("MaxCallsPerInterval")
                        .HasColumnType("int");

                    b.Property<int>("MethodName")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("QueueSetting");
                });
#pragma warning restore 612, 618
        }
    }
}
