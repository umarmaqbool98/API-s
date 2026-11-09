using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarksApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndEmailConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_users_role",
                table: "users",
                sql: "role IN ('Teacher', 'Student')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_students_email_lowercase",
                table: "students",
                sql: "email = lower(email)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_users_role",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_students_email_lowercase",
                table: "students");
        }
    }
}
