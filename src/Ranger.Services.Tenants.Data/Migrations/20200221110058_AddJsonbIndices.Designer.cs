﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Data.Migrations
{
    [DbContext(typeof(TenantsDbContext))]
    [Migration("20200221110058_AddJsonbIndices")]
    partial class AddJsonbIndices
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("FriendlyName")
                        .HasColumnName("friendly_name")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnName("xml")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys");
                });

            modelBuilder.Entity("Ranger.Services.Tenants.Data.TenantStream", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data")
                        .HasColumnType("jsonb");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnName("event")
                        .HasColumnType("text");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnName("inserted_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("InsertedBy")
                        .IsRequired()
                        .HasColumnName("inserted_by")
                        .HasColumnType("text");

                    b.Property<Guid>("StreamId")
                        .HasColumnName("stream_id")
                        .HasColumnType("uuid");

                    b.Property<int>("Version")
                        .HasColumnName("version")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_tenant_streams");

                    b.ToTable("tenant_streams");
                });

            modelBuilder.Entity("Ranger.Services.Tenants.Data.TenantUniqueConstraint", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("uuid");

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnName("domain")
                        .HasColumnType("character varying(28)")
                        .HasMaxLength(28);

                    b.HasKey("TenantId")
                        .HasName("pk_tenant_unique_constraints");

                    b.HasIndex("Domain")
                        .IsUnique();

                    b.HasIndex("TenantId")
                        .IsUnique();

                    b.ToTable("tenant_unique_constraints");
                });
#pragma warning restore 612, 618
        }
    }
}
