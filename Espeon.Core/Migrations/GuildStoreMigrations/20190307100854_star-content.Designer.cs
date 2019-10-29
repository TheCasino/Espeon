﻿// <auto-generated />

using Espeon.Core.Database.GuildStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Collections.Generic;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    [DbContext(typeof(GuildStore))]
    [Migration("20190307100854_star-content")]
    partial class starcontent
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.0.0-preview.18572.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Espeon.Core.Databases.Entities.CustomCommand", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("CustomCommand");
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Admins");

                    b.Property<decimal>("DefaultRoleId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Moderators");

                    b.Property<decimal>("NoReactions")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<List<string>>("Prefixes");

                    b.Property<string>("RestrictedChannels");

                    b.Property<string>("RestrictedUsers");

                    b.Property<string>("SelfAssigningRoles");

                    b.Property<int>("StarLimit")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(3);

                    b.Property<decimal>("StarboardChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<int>("WarningLimit")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(3);

                    b.Property<decimal>("WelcomeChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("WelcomeMessage");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.StarredMessage", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("AuthorId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("ChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Content");

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("ImageUrl");

                    b.Property<string>("ReactionUsers");

                    b.Property<decimal>("StarboardMessageId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("StarredMessage");
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.Warning", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<long>("IssuedOn")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(1551953334643L);

                    b.Property<decimal>("Issuer")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Reason");

                    b.Property<decimal>("TargetUser")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Warning");
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.CustomCommand", b =>
                {
                    b.HasOne("Espeon.Core.Databases.Entities.Guild", "Guild")
                        .WithMany("Commands")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.StarredMessage", b =>
                {
                    b.HasOne("Espeon.Core.Databases.Entities.Guild", "Guild")
                        .WithMany("StarredMessages")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Espeon.Core.Databases.Entities.Warning", b =>
                {
                    b.HasOne("Espeon.Core.Databases.Entities.Guild", "Guild")
                        .WithMany("Warnings")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
