using FluentMigrator;

namespace Quarter.Core.Migrations;

[Migration(0003)]
public class AssignTimestamps : Migration
{
    public override void Up()
    {
        Execute.Sql("UPDATE timesheet SET date_ts = DATE(date) WHERE date IS NOT NULL;");
        Execute.Sql("UPDATE timeslot SET date_ts = DATE(date) WHERE date IS NOT NULL;");
        Execute.Sql("UPDATE timeslot SET created_ts = DATE(created) WHERE created IS NOT NULL;");
    }

    public override void Down()
    {
        // NOP
    }
}
