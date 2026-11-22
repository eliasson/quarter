using FluentMigrator;

namespace Quarter.Core.Migrations;

[Migration(0004)]
public class AddUserLastLogin : Migration
{
    public override void Up()
    {
        Alter.Table("quser")
            .AddColumn("last_login_ts")
            .AsDateTimeOffset()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("last_login_ts").FromTable("quser");
    }
}
