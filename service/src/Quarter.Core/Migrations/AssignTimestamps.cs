using FluentMigrator;

namespace Quarter.Core.Migrations;

[Migration(0003)]
public class AssignTimestamps : Migration
{
    public override void Up()
    {
        Execute.Sql("UPDATE timesheet SET date_ts = TO_DATE(date, 'YYYY-MM-DD') WHERE date is not null;");
        Execute.Sql("UPDATE timeslot SET date_ts = TO_DATE(date, 'YYYY-MM-DD') WHERE date is not null;");
        Execute.Sql("UPDATE timeslot SET created_ts = TO_DATE(created, 'YYYY-MM-DD') WHERE created is not null;");
    }

    public override void Down()
    {
        // NOP
    }
}
