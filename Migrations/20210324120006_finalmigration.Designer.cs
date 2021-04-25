﻿// <auto-generated />
using System;
using IMK_web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IMK_web.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210324120006_finalmigration")]
    partial class finalmigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("IMK_web.Models.AspCompany", b =>
                {
                    b.Property<int>("AspId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApsMentorUserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CountryCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("AspId");

                    b.HasIndex("ApsMentorUserId");

                    b.HasIndex("CountryCode");

                    b.ToTable("AspCompany");
                });

            modelBuilder.Entity("IMK_web.Models.Country", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("IMK_web.Models.IMK_Functions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Alarms")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CPRI")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FRU")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IPInterfaces")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IPROUT")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RETAntenna")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RSSIFDD")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RSSINR")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RSSIUMTS")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RetSerial")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VSWR")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("IMK_Functions");
                });

            modelBuilder.Entity("IMK_web.Models.ImkVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AppVersion")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateOfRelease")
                        .HasColumnType("TEXT");

                    b.Property<int>("RPIVersion")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ImkVersions");
                });

            modelBuilder.Entity("IMK_web.Models.Log", b =>
                {
                    b.Property<string>("LogId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .HasColumnType("json");

                    b.Property<int?>("SiteVisitVisitId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LogId");

                    b.HasIndex("SiteVisitVisitId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("IMK_web.Models.Operator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CountryCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CountryCode");

                    b.ToTable("Operators");
                });

            modelBuilder.Entity("IMK_web.Models.Site", b =>
                {
                    b.Property<string>("SiteId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("AspCompanyAspId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<string>("Latitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("OperatorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("longitude")
                        .HasColumnType("TEXT");

                    b.HasKey("SiteId");

                    b.HasIndex("AspCompanyAspId");

                    b.HasIndex("OperatorId");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("IMK_web.Models.SiteVisit", b =>
                {
                    b.Property<int>("VisitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("IMK_FunctionsId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImkVersionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SiteId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("VistedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("VisitId");

                    b.HasIndex("IMK_FunctionsId");

                    b.HasIndex("ImkVersionId");

                    b.HasIndex("SiteId");

                    b.HasIndex("UserId");

                    b.ToTable("SiteVisits");
                });

            modelBuilder.Entity("IMK_web.Models.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("AspCompanyAspId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RegisteredAt")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.HasIndex("AspCompanyAspId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("IMK_web.Models.AspCompany", b =>
                {
                    b.HasOne("IMK_web.Models.User", "ApsMentor")
                        .WithMany()
                        .HasForeignKey("ApsMentorUserId");

                    b.HasOne("IMK_web.Models.Country", null)
                        .WithMany("AspCompanies")
                        .HasForeignKey("CountryCode");

                    b.Navigation("ApsMentor");
                });

            modelBuilder.Entity("IMK_web.Models.Log", b =>
                {
                    b.HasOne("IMK_web.Models.SiteVisit", null)
                        .WithMany("Logs")
                        .HasForeignKey("SiteVisitVisitId");
                });

            modelBuilder.Entity("IMK_web.Models.Operator", b =>
                {
                    b.HasOne("IMK_web.Models.Country", null)
                        .WithMany("Operators")
                        .HasForeignKey("CountryCode");
                });

            modelBuilder.Entity("IMK_web.Models.Site", b =>
                {
                    b.HasOne("IMK_web.Models.AspCompany", null)
                        .WithMany("Sites")
                        .HasForeignKey("AspCompanyAspId");

                    b.HasOne("IMK_web.Models.Operator", null)
                        .WithMany("Sites")
                        .HasForeignKey("OperatorId");
                });

            modelBuilder.Entity("IMK_web.Models.SiteVisit", b =>
                {
                    b.HasOne("IMK_web.Models.IMK_Functions", "IMK_Functions")
                        .WithMany()
                        .HasForeignKey("IMK_FunctionsId");

                    b.HasOne("IMK_web.Models.ImkVersion", "ImkVersion")
                        .WithMany()
                        .HasForeignKey("ImkVersionId");

                    b.HasOne("IMK_web.Models.Site", "Site")
                        .WithMany("SiteVisits")
                        .HasForeignKey("SiteId");

                    b.HasOne("IMK_web.Models.User", "User")
                        .WithMany("SiteVisits")
                        .HasForeignKey("UserId");

                    b.Navigation("IMK_Functions");

                    b.Navigation("ImkVersion");

                    b.Navigation("Site");

                    b.Navigation("User");
                });

            modelBuilder.Entity("IMK_web.Models.User", b =>
                {
                    b.HasOne("IMK_web.Models.AspCompany", null)
                        .WithMany("Workers")
                        .HasForeignKey("AspCompanyAspId");
                });

            modelBuilder.Entity("IMK_web.Models.AspCompany", b =>
                {
                    b.Navigation("Sites");

                    b.Navigation("Workers");
                });

            modelBuilder.Entity("IMK_web.Models.Country", b =>
                {
                    b.Navigation("AspCompanies");

                    b.Navigation("Operators");
                });

            modelBuilder.Entity("IMK_web.Models.Operator", b =>
                {
                    b.Navigation("Sites");
                });

            modelBuilder.Entity("IMK_web.Models.Site", b =>
                {
                    b.Navigation("SiteVisits");
                });

            modelBuilder.Entity("IMK_web.Models.SiteVisit", b =>
                {
                    b.Navigation("Logs");
                });

            modelBuilder.Entity("IMK_web.Models.User", b =>
                {
                    b.Navigation("SiteVisits");
                });
#pragma warning restore 612, 618
        }
    }
}
