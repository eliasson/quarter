using FluentMigrator;

namespace Quarter.Core.Migrations;

[Migration(0002)]
public class AddTimestampColumns : Migration
{
    public override void Up()
    {
        // When Quarter started as a PoC where I set the date column to a string due to pure laziness. Then I forgot
        // to change it. So add another column with the proper type and gradually remove the use of date column. Then
        // it can be renamed to just date.
        Alter.Table("timesheet")
            .AddColumn("date_ts")
            .AsDateTime()
            .Indexed()
            .Nullable();

        Alter.Table("timeslot")
            .AddColumn("date_ts")
            .AsDateTime()
            .Indexed()
            .Nullable();

        Alter.Table("timeslot")
            .AddColumn("created_ts")
            .AsDateTime()
            .Indexed()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("date_ts").FromTable("timesheet");
        Delete.Column("date_ts").FromTable("timeslot");
    }
}
