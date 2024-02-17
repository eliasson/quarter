using FluentMigrator;

namespace Quarter.Core.Migrations;

[Migration(0001)]
public class CreateTables : Migration
{
    private static readonly string[] Tables = new[] { "project", "activity", "timesheet", "timeslot", "quser" };

    public override void Up()
    {
        Create.Table("project")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("data").AsString().NotNullable()
            .WithColumn("userid").AsGuid().Indexed();

        Create.Table("activity")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("data").AsString().NotNullable()
            .WithColumn("userid").AsGuid().Indexed();

        Create.Table("timesheet")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("data").AsString().NotNullable()
            .WithColumn("date").AsString().NotNullable()
            .WithColumn("userid").AsGuid().Indexed();

        Create.Table("timeslot")
            .WithColumn("id").AsGuid().Indexed()
            .WithColumn("projectid").AsGuid().NotNullable().Indexed()
            .WithColumn("activityid").AsGuid().NotNullable().Indexed()
            .WithColumn("date").AsString().NotNullable()
            .WithColumn("created").AsString().NotNullable()
            .WithColumn("qoffset").AsInt16().NotNullable()
            .WithColumn("duration").AsInt16().NotNullable()
            .WithColumn("userid").AsGuid().Indexed();

        Create.Table("quser")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("data").AsString().NotNullable()
            .WithColumn("email").AsString().NotNullable().Unique();
    }

    public override void Down()
    {
        foreach (var aggregate in Tables)
            Delete.Table(aggregate);
    }
}
