using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GiftrMVP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExpandModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gifts_profile_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_recipients_profile_id",
                table: "recipients");

            migrationBuilder.DropColumn(
                name: "name",
                table: "recipients");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "recipients",
                newName: "profile_id");

            migrationBuilder.RenameIndex(
                name: "ix_recipients_id",
                table: "recipients",
                newName: "ix_recipients_profile_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "gifts",
                newName: "profile_id");

            migrationBuilder.RenameIndex(
                name: "ix_gifts_id",
                table: "gifts",
                newName: "ix_gifts_profile_id");

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "recipients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "recipients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Npgsql scaffolds this as a plain ALTER COLUMN ... TYPE uuid, which Postgres rejects
            // (no implicit varchar->uuid cast). Spell out the USING so the type change is legal.
            // image_assets is empty at time of writing, so the cast has no rows to trip on.
            migrationBuilder.Sql(
                "ALTER TABLE image_assets ALTER COLUMN image_key TYPE uuid USING image_key::uuid;");

            migrationBuilder.AddColumn<int>(
                name: "interest_id",
                table: "image_assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                table: "image_assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "gifts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    start_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recurring_event = table.Column<bool>(type: "boolean", nullable: false),
                    recurring_rule = table.Column<string>(type: "jsonb", nullable: true),
                    profile_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_events_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    profile_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.group_id);
                    table.ForeignKey(
                        name: "fk_groups_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interests",
                columns: table => new
                {
                    interest_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recipient_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_interests", x => x.interest_id);
                    table.ForeignKey(
                        name: "fk_interests_recipients_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "recipients",
                        principalColumn: "recipient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipient_events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    recipient_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipient_events", x => new { x.event_id, x.recipient_id });
                    table.ForeignKey(
                        name: "fk_recipient_events_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recipient_events_recipients_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "recipients",
                        principalColumn: "recipient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_events", x => new { x.event_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_group_events_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_events_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipient_groups",
                columns: table => new
                {
                    recipient_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipient_groups", x => new { x.recipient_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_recipient_groups_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recipient_groups_recipients_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "recipients",
                        principalColumn: "recipient_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_image_assets_interest_id",
                table: "image_assets",
                column: "interest_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_image_assets_profile_id",
                table: "image_assets",
                column: "profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_profile_id",
                table: "events",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_events_group_id",
                table: "group_events",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_profile_id",
                table: "groups",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_interests_recipient_id",
                table: "interests",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipient_events_recipient_id",
                table: "recipient_events",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipient_groups_group_id",
                table: "recipient_groups",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_profile_profile_id",
                table: "gifts",
                column: "profile_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_image_assets_interests_interest_id",
                table: "image_assets",
                column: "interest_id",
                principalTable: "interests",
                principalColumn: "interest_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_image_assets_profile_profile_id",
                table: "image_assets",
                column: "profile_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_recipients_profile_profile_id",
                table: "recipients",
                column: "profile_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gifts_profile_profile_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_image_assets_interests_interest_id",
                table: "image_assets");

            migrationBuilder.DropForeignKey(
                name: "fk_image_assets_profile_profile_id",
                table: "image_assets");

            migrationBuilder.DropForeignKey(
                name: "fk_recipients_profile_profile_id",
                table: "recipients");

            migrationBuilder.DropTable(
                name: "group_events");

            migrationBuilder.DropTable(
                name: "interests");

            migrationBuilder.DropTable(
                name: "recipient_events");

            migrationBuilder.DropTable(
                name: "recipient_groups");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropIndex(
                name: "ix_image_assets_interest_id",
                table: "image_assets");

            migrationBuilder.DropIndex(
                name: "ix_image_assets_profile_id",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "recipients");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "recipients");

            migrationBuilder.DropColumn(
                name: "interest_id",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "location",
                table: "gifts");

            migrationBuilder.RenameColumn(
                name: "profile_id",
                table: "recipients",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "ix_recipients_profile_id",
                table: "recipients",
                newName: "ix_recipients_id");

            migrationBuilder.RenameColumn(
                name: "profile_id",
                table: "gifts",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "ix_gifts_profile_id",
                table: "gifts",
                newName: "ix_gifts_id");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "recipients",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            // Reverse of the Up cast. uuid renders as its canonical 36-char text form here.
            migrationBuilder.Sql(
                "ALTER TABLE image_assets ALTER COLUMN image_key TYPE character varying(32) USING image_key::text;");

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_profile_id",
                table: "gifts",
                column: "id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_recipients_profile_id",
                table: "recipients",
                column: "id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
