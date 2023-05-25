﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VRGardenAlpha.Data;

#nullable disable

namespace VRGardenAlpha.Migrations
{
    [DbContext(typeof(GardenContext))]
    partial class GardenContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VRGardenAlpha.Data.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ACL")
                        .HasColumnType("integer");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<IPAddress>("AuthorIP")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.Property<string>("Checksum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Chunks")
                        .HasColumnType("integer");

                    b.Property<long>("ContentLength")
                        .HasColumnType("bigint");

                    b.Property<string>("ContentLink")
                        .HasColumnType("text");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Creator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("Downloads")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<List<string>>("Features")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("ImageContentLength")
                        .HasColumnType("bigint");

                    b.Property<string>("ImageContentType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("LastChunk")
                        .HasColumnType("integer");

                    b.Property<byte>("Platform")
                        .HasColumnType("smallint");

                    b.Property<string>("RemoteId")
                        .HasColumnType("text");

                    b.Property<List<string>>("Tags")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Views")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("VRGardenAlpha.Data.Trade", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("ACL")
                        .HasColumnType("integer");

                    b.Property<TradeDetails>("Initiator")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string[]>("InitiatorPaths")
                        .HasColumnType("text[]");

                    b.Property<TradeDetails>("Recipient")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string[]>("RecipientPaths")
                        .HasColumnType("text[]");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Trades");
                });
#pragma warning restore 612, 618
        }
    }
}
