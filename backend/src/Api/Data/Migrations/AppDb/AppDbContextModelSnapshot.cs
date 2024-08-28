﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

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
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        });
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

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

                    b.Property<DateTimeOffset>("RegistrationDate")
                        .HasColumnType("timestamp with time zone");

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

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationUserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.CollectionItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CollectionParentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeviceCollectionId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SubCollectionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CollectionParentId");

                    b.HasIndex("DeviceCollectionId");

                    b.HasIndex("DeviceId");

                    b.HasIndex("SubCollectionId");

                    b.ToTable("CollectionItems");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.CollectionShare", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CollectionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("UserId");

                    b.ToTable("CollectionShare");
                });

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

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AccessToken")
                        .IsUnique();

                    b.HasIndex("DeviceTemplateId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceCollection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsRoot")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RootCollectionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("RootCollectionId");

                    b.ToTable("DeviceCollections");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

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

                    b.Property<int>("CurrentCycle")
                        .HasColumnType("integer");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("integer");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Paused")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("TotalCycles")
                        .HasColumnType("integer");

                    b.Property<int>("TotalSteps")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.JobCommand", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("JobId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<Guid>("OriginalCommandId")
                        .HasColumnType("uuid");

                    b.Property<List<double>>("Params")
                        .IsRequired()
                        .HasColumnType("double precision[]");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("JobCommands");
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

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

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

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationUserRole", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationRole", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.CollectionItem", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceCollection", "CollectionParent")
                        .WithMany("ChildItems")
                        .HasForeignKey("CollectionParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.DeviceCollection", null)
                        .WithMany("AllCollectionsFlat")
                        .HasForeignKey("DeviceCollectionId");

                    b.HasOne("Fei.Is.Api.Data.Models.Device", "Device")
                        .WithMany("CollectionItems")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Fei.Is.Api.Data.Models.DeviceCollection", "SubCollection")
                        .WithMany("ParentItems")
                        .HasForeignKey("SubCollectionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("CollectionParent");

                    b.Navigation("Device");

                    b.Navigation("SubCollection");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.CollectionShare", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.DeviceCollection", "Collection")
                        .WithMany("CollectionShares")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "User")
                        .WithMany("CollectionShares")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Collection");

                    b.Navigation("User");
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

                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "Owner")
                        .WithMany("Devices")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceTemplate");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceCollection", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.DeviceCollection", "RootCollection")
                        .WithMany()
                        .HasForeignKey("RootCollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("RootCollection");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceTemplate", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "Owner")
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

            modelBuilder.Entity("Fei.Is.Api.Data.Models.JobCommand", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.Job", "Job")
                        .WithMany("Commands")
                        .HasForeignKey("JobId")
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
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Fei.Is.Api.Data.Models.Recipe", "Recipe")
                        .WithMany("Steps")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fei.Is.Api.Data.Models.Recipe", "Subrecipe")
                        .WithMany("ParentSteps")
                        .HasForeignKey("SubrecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Command");

                    b.Navigation("Recipe");

                    b.Navigation("Subrecipe");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.RefreshToken", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", "User")
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

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("Fei.Is.Api.Data.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationRole", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.ApplicationUser", b =>
                {
                    b.Navigation("CollectionShares");

                    b.Navigation("DeviceTemplates");

                    b.Navigation("Devices");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Command", b =>
                {
                    b.Navigation("RecipeSteps");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Device", b =>
                {
                    b.Navigation("CollectionItems");

                    b.Navigation("Jobs");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.DeviceCollection", b =>
                {
                    b.Navigation("AllCollectionsFlat");

                    b.Navigation("ChildItems");

                    b.Navigation("CollectionShares");

                    b.Navigation("ParentItems");
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
                    b.Navigation("Commands");
                });

            modelBuilder.Entity("Fei.Is.Api.Data.Models.Recipe", b =>
                {
                    b.Navigation("ParentSteps");

                    b.Navigation("Steps");
                });
#pragma warning restore 612, 618
        }
    }
}
