using System;

namespace Quarter
{
    public static class Page
    {
        public static string Home => "/app";
        public static string TimesheetBase => "/app/timesheet/";
        public static string Manage => "/app/manage/projects";
        public static string Report => "/app/reports";
        public static string Profile => "/app/profile";
        public static string Admin => "/admin";
        public static string AdminUsers => "/admin/users";
        public static string Logout => "/account/logout";
        public static string Login => "/account/login";

        public static string Timesheet(DateTime date)
            => $"/app/timesheet/{date:yyyy-MM-dd}";
    }
}