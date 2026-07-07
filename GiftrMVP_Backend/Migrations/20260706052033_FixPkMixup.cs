using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GiftrMVP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixPkMixup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gifts_profile_profile_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_gifts_recipients_recipient_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_image_assets_gifts_gift_id",
                table: "image_assets");

            migrationBuilder.DropForeignKey(
                name: "fk_recipients_profile_profile_id",
                table: "recipients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_recipients",
                table: "recipients");

            migrationBuilder.DropIndex(
                name: "ix_recipients_profile_id",
                table: "recipients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_gifts",
                table: "gifts");

            migrationBuilder.DropIndex(
                name: "ix_gifts_profile_id",
                table: "gifts");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "recipients");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "gifts");

            migrationBuilder.AlterColumn<int>(
                name: "recipient_id",
                table: "recipients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "recipients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "gift_id",
                table: "gifts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "gifts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_recipients",
                table: "recipients",
                column: "recipient_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_gifts",
                table: "gifts",
                column: "gift_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipients_id",
                table: "recipients",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_gifts_id",
                table: "gifts",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_profile_id",
                table: "gifts",
                column: "id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_recipients_recipient_id",
                table: "gifts",
                column: "recipient_id",
                principalTable: "recipients",
                principalColumn: "recipient_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_image_assets_gifts_gift_id",
                table: "image_assets",
                column: "gift_id",
                principalTable: "gifts",
                principalColumn: "gift_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_recipients_profile_id",
                table: "recipients",
                column: "id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gifts_profile_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_gifts_recipients_recipient_id",
                table: "gifts");

            migrationBuilder.DropForeignKey(
                name: "fk_image_assets_gifts_gift_id",
                table: "image_assets");

            migrationBuilder.DropForeignKey(
                name: "fk_recipients_profile_id",
                table: "recipients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_recipients",
                table: "recipients");

            migrationBuilder.DropIndex(
                name: "ix_recipients_id",
                table: "recipients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_gifts",
                table: "gifts");

            migrationBuilder.DropIndex(
                name: "ix_gifts_id",
                table: "gifts");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "recipients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "recipient_id",
                table: "recipients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                table: "recipients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "gifts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "gift_id",
                table: "gifts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                table: "gifts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_recipients",
                table: "recipients",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_gifts",
                table: "gifts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_recipients_profile_id",
                table: "recipients",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_gifts_profile_id",
                table: "gifts",
                column: "profile_id");

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_profile_profile_id",
                table: "gifts",
                column: "profile_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_gifts_recipients_recipient_id",
                table: "gifts",
                column: "recipient_id",
                principalTable: "recipients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_image_assets_gifts_gift_id",
                table: "image_assets",
                column: "gift_id",
                principalTable: "gifts",
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
    }
}
