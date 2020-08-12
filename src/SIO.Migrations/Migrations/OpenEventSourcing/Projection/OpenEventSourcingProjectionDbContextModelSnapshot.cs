﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenEventSourcing.EntityFrameworkCore.DbContexts;

namespace SIO.Migrations.Migrations.OpenEventSourcing.Projection
{
    [DbContext(typeof(OpenEventSourcingProjectionDbContext))]
    partial class OpenEventSourcingProjectionDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("OpenEventSourcing.EntityFrameworkCore.Entities.ProjectionState", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<DateTimeOffset?>("LastModifiedDate");

                    b.Property<long>("Position");

                    b.HasKey("Name");

                    b.ToTable("ProjectionState");
                });
#pragma warning restore 612, 618
        }
    }
}
