﻿// <auto-generated />

using Espeon.Core.Database.UserStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Espeon.Core.Migrations
{
    [DbContext(typeof(UserStore))]
    [Migration("20190408001113_guid")]
    partial class guid
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.0.0-preview3.19153.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Espeon.Core.Databases.Reminder", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("ChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("JumpUrl");

                    b.Property<int>("ReminderId");

                    b.Property<string>("TaskKey")
                        .IsRequired();

                    b.Property<string>("TheReminder");

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<long>("WhenToRemove");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Espeon.Core.Databases.User", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<int>("CandyAmount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(10);

                    b.Property<int>("HighestCandies")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(10);

                    b.Property<long>("LastClaimedCandies");

                    b.Property<int>("ResponsePack")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int[]>("ResponsePacks")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new[] { 0 });

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Espeon.Core.Databases.Reminder", b =>
                {
                    b.HasOne("Espeon.Core.Databases.User")
                        .WithMany("Reminders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
