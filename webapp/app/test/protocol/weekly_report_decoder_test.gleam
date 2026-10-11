import domain/duration
import domain/project
import gleam/json
import gleam/list
import gleeunit/should
import protocol

pub fn decode_minimal_weekly_report_test() {
  let json_string =
    "{\"startOfWeek\":\"2026-03-16\",\"endOfWeek\":\"2026-03-22\",\"totalMinutes\":0,\"weekdayTotals\":[0,0,0,0,0,0,0],\"usage\":[]}"

  let result =
    json.parse(json_string, protocol.weekly_report_decoder())
    |> should.be_ok

  result.duration |> should.equal(duration.Minutes(0))
  result.usage |> should.equal([])
  result.weekday_totals |> should.equal(list.repeat(duration.Minutes(0), 7))
}

pub fn decode_weekly_report_totals_test() {
  let result =
    json.parse(weekly_report_json, protocol.weekly_report_decoder())
    |> should.be_ok

  result.duration |> should.equal(duration.Minutes(480))
  result.weekday_totals
  |> should.equal([
    duration.Minutes(360),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(120),
  ])
}

pub fn decode_weekly_report_usage_test() {
  let result =
    json.parse(weekly_report_json, protocol.weekly_report_decoder())
    |> should.be_ok

  result.usage
  |> list.length
  |> should.equal(1)

  let project_usage =
    list.first(result.usage)
    |> should.be_ok()

  project_usage.project_id
  |> should.equal(project.ProjectId("62115e0a-bfd0-46f8-88f7-6ddf70777637"))

  project_usage.duration |> should.equal(duration.Minutes(480))
}

pub fn decode_weekly_report_activity_usage_test() {
  let result =
    json.parse(weekly_report_json, protocol.weekly_report_decoder())
    |> should.be_ok

  let project_usage =
    list.first(result.usage)
    |> should.be_ok()

  project_usage.activity_usage
  |> list.length
  |> should.equal(1)

  let activity_usage =
    list.first(project_usage.activity_usage)
    |> should.be_ok()

  activity_usage.activity_id
  |> should.equal(project.ActivityId("3ce56e95-12a4-4598-8ff8-a8c5fce4e016"))

  activity_usage.duration |> should.equal(duration.Minutes(480))

  activity_usage.weekday_totals
  |> should.equal([
    duration.Minutes(360),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(0),
    duration.Minutes(120),
  ])
}

const weekly_report_json = "{
  \"startOfWeek\": \"2026-03-16\",
  \"endOfWeek\": \"2026-03-22\",
  \"totalMinutes\": 480,
  \"weekdayTotals\": [360, 0, 0, 0, 0, 0, 120],
  \"usage\": [
    {
      \"projectId\": \"62115e0a-bfd0-46f8-88f7-6ddf70777637\",
      \"totalMinutes\": 480,
      \"activityUsage\": [
        {
          \"activityId\": \"3ce56e95-12a4-4598-8ff8-a8c5fce4e016\",
          \"totalMinutes\": 480,
          \"weekdayTotals\": [360, 0, 0, 0, 0, 0, 120]
        }
      ]
    }
  ]
}"
