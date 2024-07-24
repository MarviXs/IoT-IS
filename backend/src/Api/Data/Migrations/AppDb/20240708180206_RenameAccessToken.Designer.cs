﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240708180206_RenameAccessToken")]
    partial class RenameAccessToken
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Command", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeviceTemplateId")
                        .HasColumnType("uuid");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<double>>("Params")
                        .IsRequired()
                        .HasColumnType("double precision[]");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceTemplateId");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeviceTemplateId")
                        .HasColumnType("uuid");

                    b.Property<string>("Mac")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceTemplateId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ModelId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("OwnerId");

                    b.ToTable("DeviceTemplates");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Job", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("NoOfCmds")
                        .HasColumnType("integer");

                    b.Property<int>("NoOfReps")
                        .HasColumnType("integer");

                    b.Property<bool>("Paused")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("ToCancel")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.JobStatus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Code")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("CurrentCycle")
                        .HasColumnType("integer");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("integer");

                    b.Property<Guid>("JobId")
                        .HasColumnType("uuid");

                    b.Property<int>("RetCode")
                        .HasColumnType("integer");

                    b.Property<int>("TotalSteps")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("JobId")
                        .IsUnique();

                    b.ToTable("JobStatuses");

                    b.HasAnnotation("Npgsql:StorageParameter:fillfactor", 70);
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Recipe", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeviceTemplateId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceTemplateId");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.RecipeStep", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CommandId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Cycles")
                        .HasColumnType("integer");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<Guid>("RecipeId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SubrecipeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CommandId");

                    b.HasIndex("RecipeId");

                    b.HasIndex("SubrecipeId");

                    b.ToTable("RecipeSteps");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("CreatedAt")
                        .HasColumnType("bigint");

                    b.Property<long>("ExpiresAt")
                        .HasColumnType("bigint");

                    b.Property<Guid>("Token")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("AccuracyDecimals")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeviceTemplateId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Unit")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceTemplateId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "75269909-d757-4f09-8bab-62db2b072bea",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Command", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceTemplate", "DeviceTemplate")
                        .WithMany("Commands")
                        .HasForeignKey("DeviceTemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceTemplate");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Device", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceTemplate", "DeviceTemplate")
                        .WithMany("Devices")
                        .HasForeignKey("DeviceTemplateId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Fei.Is.Api.Data.Models.User", "Owner")
                        .WithMany("Devices")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceTemplate");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceTemplate", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.User", "Owner")
                        .WithMany("DeviceTemplates")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Job", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.Device", "Device")
                        .WithMany("Jobs")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.JobStatus", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.Job", "Job")
                        .WithOne("Status")
                        .HasForeignKey("Fei.Is.Api.Data.Models.JobStatus", "JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Recipe", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceTemplate", "DeviceTemplate")
                        .WithMany("Recipes")
                        .HasForeignKey("DeviceTemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceTemplate");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.RecipeStep", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.Command", "Command")
                        .WithMany("RecipeSteps")
                        .HasForeignKey("CommandId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Fei.Is.Api.Data.Models.Recipe", "Recipe")
                        .WithMany("Steps")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.Recipe", "Subrecipe")
                        .WithMany("ParentSteps")
                        .HasForeignKey("SubrecipeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Command");

                    b.Navigation("Recipe");

                    b.Navigation("Subrecipe");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.RefreshToken", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Sensor", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceTemplate", "DeviceTemplate")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceTemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceTemplate");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Command", b =>
                {
                    b.Navigation("RecipeSteps");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Device", b =>
                {
                    b.Navigation("Jobs");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceTemplate", b =>
                {
                    b.Navigation("Commands");

                    b.Navigation("Devices");

                    b.Navigation("Recipes");

                    b.Navigation("Sensors");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Job", b =>
                {
                    b.Navigation("Status");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Recipe", b =>
                {
                    b.Navigation("ParentSteps");

                    b.Navigation("Steps");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.User", b =>
                {
                    b.Navigation("DeviceTemplates");

                    b.Navigation("Devices");
                });
#pragma warning restore 612, 618
        }
    }
}
