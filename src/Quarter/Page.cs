using System;

namespace Quarter;

public static class Page
{
    public static string Home => "/app";
    public static string TimesheetBase => "/app/timesheet/";
    public static string Manage => "/app/manage/projects";
    public static string WeeklyReport => "/app/reports";
    public static string MonhtlyReport => "/app/reports/month";
    public static string Profile => "/app/profile";
    public static string Admin => "/admin";
    public static string AdminUsers => "/admin/users";
    public static string Logout => "/account/logout";
    public static string Login => "/account/login";

    public static string Timesheet(DateTime date)
        => $"/app/timesheet/{date:yyyy-MM-dd}";

    public static string WeekReport(DateTime date)
        => $"/app/reports/{date:yyyy-MM-dd}";

    public static string MonthlyReport(DateTime date)
        => $"/app/reports/month/{date:yyyy-MM-dd}";
}